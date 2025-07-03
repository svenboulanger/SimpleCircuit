using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram
{
    /// <summary>
    /// An action for an Entity-Relationship Diagram.
    /// </summary>
    [Drawable("ACT", "An entity-relationship diagram action.", "ERD", "diamond")]
    public class Action : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new action.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        private class Instance(string name) : DiagramBlockInstance(name)
        {
            private double _width, _height;
            private readonly CustomLabelAnchorPoints _anchors = new(1);

            /// <inheritdoc />
            public override string Type => "action";

            [Description("The width of the block.")]
            [Alias("w")]
            public double Width { get; set; } = 0;

            [Description("The minimum width of the block. Only used when determining the width from contents.")]
            public double MinWidth { get; set; } = 0.0;

            [Description("The height of the block.")]
            [Alias("h")]
            public double Height { get; set; } = 0;

            [Description("The minimum height of the block. Only used when determining the height from contents.")]
            public double MinHeight { get; set; } = 10.0;

            [Description("The margin of the label inside the action when sizing based on content.")]
            public Margins Margin { get; set; } = new(2, 2, 2, 2);

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            [Description("The corner radius for the left and right corner.")]
            [Alias("rx")]
            public double CornerRadiusX { get; set; }

            [Description("The corner radius for the top and bottom corner.")]
            [Alias("ry")]
            public double CornerRadiusY { get; set; }

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                // When determining the size, let's update the size based on the label bounds
                switch (context.Mode)
                {
                    case PreparationMode.Sizes:
                        var style = context.Style.ModifyDashedDotted(this);

                        if (Width.IsZero() || Height.IsZero())
                        {
                            var bounds = LabelAnchorPoints<IDrawable>.CalculateBounds(context.TextFormatter, this, 0, _anchors, style);
                            bounds = bounds.Expand(Margin).Expand(style.LineThickness * 0.5);

                            if (Width.IsZero() && Height.IsZero())
                            {
                                _width = Math.Max(MinWidth, bounds.Width * 2);
                                _height = Math.Max(MinHeight, bounds.Height * 2);
                            }
                            else if (Width.IsZero())
                            {
                                _height = Height;
                                _width = Math.Max(MinWidth, bounds.Width * Height / (Height - bounds.Height));
                                if (_width < 0)
                                    _width = bounds.Width * 2;
                            }
                            else
                            {
                                _width = Width;
                                _height = Math.Max(MinHeight, bounds.Height * Width / (Width - bounds.Width));
                                if (_height < 0)
                                    _height = bounds.Height * 2;
                            }

                            _anchors[0] = new LabelAnchorPoint(-bounds.Center, Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.Origin);
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                builder.Diamond(0.0, 0.0, _width, _height, style, CornerRadiusX, CornerRadiusY);
                _anchors.Draw(builder, this, style);
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                CommonGraphical.DiamondSize(_width, _height, CornerRadiusX, CornerRadiusY, out var _, out var ox, out var oy);
                Vector2 Interp(DiamondLocation l1, DiamondLocation l2, double ka)
                {
                    Vector2 a = CommonGraphical.GetDiamondOffset(_width, _height, ox, oy, l1);
                    Vector2 b = CommonGraphical.GetDiamondOffset(_width, _height, ox, oy, l2);
                    double k = ka / (Math.PI * 0.5);
                    return (1 - k) * a + k * b;
                }
                double a = _width * 0.5;
                double b = _height * 0.5;

                foreach (var pin in pins)
                {
                    if (pin.Orientation.X < -0.999)
                        pin.Offset = new(-a, 0);
                    else if (pin.Orientation.X > 0.999)
                        pin.Offset = new(a, 0);
                    else if (pin.Orientation.Y < -0.999)
                        pin.Offset = new(0, -b);
                    else if (pin.Orientation.Y > 0.999)
                        pin.Offset = new(0, b);
                    else
                    {
                        double alpha = Math.Atan2(pin.Orientation.Y, pin.Orientation.X);
                        if (alpha < -Math.PI * 0.5)
                            pin.Offset = Interp(DiamondLocation.TopLeftLeft, DiamondLocation.TopLeftTop, alpha + Math.PI);
                        else if (alpha < 0)
                            pin.Offset = Interp(DiamondLocation.TopRightTop, DiamondLocation.TopRightRight, alpha + Math.PI * 0.5);
                        else if (alpha < 0.5 * Math.PI)
                            pin.Offset = Interp(DiamondLocation.BottomRightRight, DiamondLocation.BottomRightBottom, alpha);
                        else
                            pin.Offset = Interp(DiamondLocation.BottomLeftBottom, DiamondLocation.BottomLeftLeft, alpha - Math.PI * 0.5);
                    }
                }
            }
        }
    }
}
