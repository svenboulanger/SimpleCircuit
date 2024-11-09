using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram
{
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
        private class Instance(string name) : DiagramBlockInstance(name), ILabeled, IBoxLabeled, IRoundedDiamond
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

            /// <inheritdoc />
            public override string Type => "action";

            [Description("The width of the block.")]
            [Alias("w")]
            public double Width { get; set; } = 40;

            [Description("The height of the block.")]
            [Alias("h")]
            public double Height { get; set; } = 20;

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            [Description("The corner radius for the left and right corner.")]
            [Alias("rx")]
            public double CornerRadiusX { get; set; }

            [Description("The corner radius for the top and bottom corner.")]
            [Alias("ry")]
            public double CornerRadiusY { get; set; }

            Vector2 IBoxLabeled.TopLeft => new(-Width * 0.5, -Height * 0.5);
            Vector2 IBoxLabeled.BottomRight => new(Width * 0.5, Height * 0.5);

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.Diamond(0.0, 0.0, Width, Height, CornerRadiusX, CornerRadiusY);
                DiamondLabelAnchorPoints.Default.Draw(builder, this);
            }


            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                CommonGraphical.DiamondSize(Width, Height, CornerRadiusX, CornerRadiusY, out var _, out var ox, out var oy);
                Vector2 Interp(DiamondLocation l1, DiamondLocation l2, double ka)
                {
                    Vector2 a = CommonGraphical.GetDiamondOffset(Width, Height, ox, oy, l1);
                    Vector2 b = CommonGraphical.GetDiamondOffset(Width, Height, ox, oy, l2);
                    double k = ka / (Math.PI * 0.5);
                    return (1 - k) * a + k * b;
                }
                double a = Width * 0.5;
                double b = Height * 0.5;

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
