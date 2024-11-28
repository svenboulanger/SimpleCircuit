using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.FlowChart
{
    [Drawable("FP", "A Flowchart process.", "Flowchart", "box rectangle")]
    public class Process : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : DiagramBlockInstance(name), IBoxDrawable
        {
            public const string Predefined = "predefined";
            private double _width = 0, _height = 0;

            /// <inheritdoc />
            public override string Type => "process";

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            [Description("The width of the block. If 0, the width is calculated using the text inside.")]
            [Alias("w")]
            public double Width { get; set; } = 0.0;

            /// <summary>
            /// Gets or sets the minimum width.
            /// </summary>
            [Description("The minimum width of the block. Only used when Width = 0.")]
            public double MinWidth { get; set; } = 0.0;

            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            [Description("The height of the block.")]
            [Alias("h")]
            public double Height { get; set; } = 0.0;

            /// <summary>
            /// Gets or sets the minimum height.
            /// </summary>
            [Description("The minimum height of the block. Only used when Height = 0.")]
            public double MinHeight { get; set; } = 10.0;

            /// <summary>
            /// Gets or sets the margin used for the label to the edge.
            /// </summary>
            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            [Description("The margin used when sizing the block using the content.")]
            public Margins Margin { get; set; } = new(2, 2, 2, 2);

            [Description("The spacing used between multiple labels when sizing the block using content.")]
            public Vector2 Spacing { get; set; } = new(3, 3);

            /// <summary>
            /// Gets or sets the radius used for the edges.
            /// </summary>
            [Description("The round-off corner radius.")]
            [Alias("r")]
            [Alias("radius")]
            public double CornerRadius { get; set; }

            Vector2 IBoxDrawable.TopLeft => -0.5 * new Vector2(_width, _height);
            Vector2 IBoxDrawable.BottomRight => 0.5 * new Vector2(_width, _height);

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
                            var b = BoxLabelAnchorPoints.Default.CalculateSize(this, Spacing);
                            _width = Math.Max(MinWidth, b.X + Margin.Left + Margin.Right + CornerRadius * 0.707 * 2);
                            _height = Math.Max(MinHeight, b.Y + Margin.Top + Margin.Bottom + CornerRadius * 0.707 * 2);
                        }
                        else
                        {
                            _width = Width;
                            _height = Height;
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.Rectangle(-_width * 0.5, -_height * 0.5, _width, _height, CornerRadius, CornerRadius);

                if (Variants.Contains(Predefined))
                {
                    double a = _width * 0.5;
                    double b = _height * 0.5;
                    builder.Line(new(-a + 3, -b), new(-a + 3, b));
                    builder.Line(new(a - 3, -b), new(a - 3, b));
                }
                BoxLabelAnchorPoints.Default.Draw(builder, this);
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                double a = Width * 0.5;
                double b = Height * 0.5;

                static Vector2 Interp(Vector2 a, Vector2 b, double ka)
                {
                    double k = ka / (Math.PI * 0.5);
                    return (1 - k) * a + k * b;
                }

                foreach (var pin in pins)
                {
                    double angle = Math.Atan2(pin.Orientation.Y, pin.Orientation.X);

                    // Deal with the corners first
                    if (Math.Abs(angle + Math.PI * 0.75) < 1e-3)
                    {
                        double k = 0.29289321881 * CornerRadius;
                        pin.Offset = new(-a + k, -b + k);
                    }
                    else if (Math.Abs(angle + Math.PI * 0.25) < 1e-3)
                    {
                        double k = 0.29289321881 * CornerRadius;
                        pin.Offset = new(a - k, -b + k);
                    }
                    else if (Math.Abs(angle - Math.PI * 0.25) < 1e-3)
                    {
                        double k = 0.29289321881 * CornerRadius;
                        pin.Offset = new(a - k, b - k);
                    }
                    else if (Math.Abs(angle - Math.PI * 0.75) < 1e-3)
                    {
                        double k = 0.29289321881 * CornerRadius;
                        pin.Offset = new(-a + k, b - k);
                    }
                    else if (angle < -Math.PI * 0.75)
                        pin.Offset = Interp(new(-a, b - CornerRadius), new(-a, -b + CornerRadius), angle + Math.PI * 1.25);
                    else if (angle < -Math.PI * 0.25)
                        pin.Offset = Interp(new(-a + CornerRadius, -b), new(a - CornerRadius, -b), angle + Math.PI * 0.75);
                    else if (angle < Math.PI * 0.25)
                        pin.Offset = Interp(new(a, -b + CornerRadius), new(a, b - CornerRadius), angle + Math.PI * 0.25);
                    else if (angle < Math.PI * 0.75)
                        pin.Offset = Interp(new(a - CornerRadius, b), new(-a + CornerRadius, b), angle - Math.PI * 0.25);
                    else
                        pin.Offset = Interp(new(-a, b - CornerRadius), new(-a, -b + CornerRadius), angle - Math.PI * 0.75);
                }
            }
        }
    }
}
