using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
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

            /// <inheritdoc />
            public override string Type => "process";

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            [Description("The width of the block.")]
            [Alias("w")]
            public double Width { get; set; } = 30.0;

            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            [Description("The height of the block.")]
            [Alias("h")]
            public double Height { get; set; } = 15.0;

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            [Description("The round-off corner radius.")]
            [Alias("r")]
            [Alias("radius")]
            public double CornerRadius { get; set; }

            Vector2 IBoxDrawable.TopLeft => -0.5 * new Vector2(Width, Height);
            Vector2 IBoxDrawable.BottomRight => 0.5 * new Vector2(Width, Height);

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.Rectangle(-Width * 0.5, -Height * 0.5, Width, Height, CornerRadius, CornerRadius);

                if (Variants.Contains(Predefined))
                {
                    double a = Width * 0.5;
                    double b = Height * 0.5;
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
