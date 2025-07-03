using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.FlowChart
{
    /// <summary>
    /// A flowchart document.
    /// </summary>
    [Drawable("FDOC", "A Flowchart document.", "Flowchart")]
    public class Document : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : DiagramBlockInstance(name)
        {
            private double _width = 0.0, _height = 0.0;
            private readonly CustomLabelAnchorPoints _anchors = new(1);

            /// <summary>
            /// Variant for multiple documents.
            /// </summary>
            public const string Multiple = "multiple";

            /// <inheritdoc />
            public override string Type => "document";

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            [Description("The width of the block.")]
            [Alias("w")]
            public double Width { get; set; }

            /// <summary>
            /// Gets or sets the minimum width.
            /// </summary>
            [Description("The minimum width of the block. Only used when determining the width from contents.")]
            public double MinWidth { get; set; } = 0.0;

            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            [Description("The height of the block.")]
            [Alias("h")]
            public double Height { get; set; }

            /// <summary>
            /// Gets or sets the minimum height.
            /// </summary>
            [Description("The minimum height of the block. Only used when determining the height from contents.")]
            public double MinHeight { get; set; } = 10.0;

            /// <summary>
            /// Gets or sets the margin for content when sizing.
            /// </summary>
            [Description("The margin used when sizing the block using the contents. The default is 2 on all sides.")]
            public Margins Margin { get; set; } = new(2, 2, 2, 2);

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
                        if (Width.IsZero() || Height.IsZero())
                        {
                            var style = context.Style.ModifyDashedDotted(this);
                            var bounds = LabelAnchorPoints<IDrawable>.CalculateBounds(context.TextFormatter, this, 0, _anchors, style);
                            bounds = bounds.Expand(Margin).Expand(style.LineThickness * 0.5);
                            if (Width.IsZero())
                                _width = Math.Max(MinWidth, bounds.Width);
                            else
                                _width = Width;
                            if (Height.IsZero())
                                _height = Math.Max(MinHeight, bounds.Height);
                            else
                                _height = Height;

                            _anchors[0] = new LabelAnchorPoint(-bounds.Center, Vector2.NaN, Vector2.UX, TextOrientationType.Transformed);
                        }
                        else
                        {
                            _width = Width;
                            _height = Height;
                            _anchors[0] = new LabelAnchorPoint(Vector2.Zero, Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.Center);
                        }
                        break;
                }
                return result;
            }

            private void DrawPath(IPathBuilder builder)
            {
                double a = _width * 0.5;
                double b = _height * 0.5;

                Vector2 h1 = new Vector2(0.1547, 0.0893) * _width;
                Vector2 aa = new(-a, b + h1.Y * 0.75);
                Vector2 ab = new(0, aa.Y);
                Vector2 ac = new(a, aa.Y);
                Vector2 h2 = new(h1.X, -h1.Y);
                builder.MoveTo(new(-a, -b))
                    .LineTo(new(a, -b))
                    .LineTo(ac)
                    .CurveTo(ac - h1, ab + h2, ab)
                    .CurveTo(ab - h2, aa + h1, aa)
                    .Close();
            }
            private void DrawPartial(IPathBuilder builder)
            {
                double a = _width * 0.5;
                double b = _height * 0.5;
                Vector2 h1 = new Vector2(0.1547, 0.0893) * _width;
                Vector2 aa = new(-a, b + h1.Y * 0.75);
                Vector2 ab = new(0, aa.Y);
                Vector2 ac = new(a, aa.Y);
                Vector2 h2 = new(h1.X, -h1.Y);
                builder.MoveTo(new(-a, -b))
                    .Horizontal(_width)
                    .Vertical(2)
                    .HorizontalTo(-a + 2)
                    .Vertical(_height - 1)
                    .Curve(new(-1, 0), new Vector2(-2, -0.5), new(-2, -0.5))
                    .Close();
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);

                if (Variants.Contains(Multiple))
                {
                    // Draw multiple paths behind it
                    for (int i = 2; i >= 1; i--)
                    {
                        builder.BeginTransform(new(new(-i * 2, -i * 2), Matrix2.Identity));
                        builder.Path(DrawPartial, style);
                        builder.EndTransform();
                    }
                }
                builder.Path(DrawPath, style);
                _anchors.Draw(builder, this, style);
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                double a = _width * 0.5;
                double b = _height * 0.5;

                static Vector2 Interp(Vector2 a, Vector2 b, double ka)
                {
                    double k = ka / (Math.PI * 0.5);
                    return (1 - k) * a + k * b;
                }


                foreach (var pin in pins)
                {
                    double angle = Math.Atan2(pin.Orientation.Y, pin.Orientation.X);
                    if (angle < -Math.PI * 0.75)
                        pin.Offset = Interp(new(-a, b), new(-a, -b), angle + Math.PI * 1.25);
                    else if (angle < -Math.PI * 0.25)
                        pin.Offset = Interp(new(-a, -b), new(a, -b), angle + Math.PI * 0.75);
                    else if (angle < Math.PI * 0.25)
                        pin.Offset = Interp(new(a, -b), new(a, b), angle + Math.PI * 0.25);
                    else if (angle < Math.PI * 0.5)
                    {
                        angle = (angle - Math.PI * 0.25) * 4.0 / 3.0;
                        Vector2 center = new(a * 0.5, b + a * Math.Sqrt(3) / 2);
                        pin.Offset = center + Vector2.Normal(-Math.PI / 3 - angle) * a;
                    }
                    else if (angle < Math.PI * 0.75)
                    {
                        angle = (angle - Math.PI * 0.5) * 4.0 / 3.0;
                        Vector2 center = new(-a * 0.5, b - a * Math.Sqrt(3) / 2);
                        pin.Offset = center + Vector2.Normal(Math.PI / 3 + angle) * a;
                    }
                    else
                        pin.Offset = Interp(new(-a, b), new(-a, -b), angle - Math.PI * 0.75);
                }
            }
        }
    }
}
