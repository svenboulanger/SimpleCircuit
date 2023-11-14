using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.FlowChart
{
    [Drawable("FDOC", "A Flowchart document.", "Flowchart")]
    public class Document : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : DiagramBlockInstance, ILabeled, IBoxLabeled
        {
            private double _width = 30.0, _height = 15.0;

            /// <summary>
            /// Variant for multiple documents.
            /// </summary>
            public const string Multiple = "multiple";

            /// <inheritdoc />
            public override string Type => "document";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            [Description("The width of the block.")]
            [Alias("w")]
            public double Width
            {
                get => _width;
                set => _width = value;
            }

            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            [Description("The height of the block.")]
            [Alias("h")]
            public double Height
            {
                get => _height;
                set => _height = value;
            }

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            Vector2 IBoxLabeled.TopLeft => new(-Width * 0.5, -Height * 0.5);
            Vector2 IBoxLabeled.BottomRight => new(Width * 0.5, Height * 0.5);

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
            }

            private void DrawPath(PathBuilder builder)
            {
                double a = Width * 0.5;
                double b = Height * 0.5;
                double r = Width * 0.5;

                Vector2 aa = new(-a, b);
                Vector2 ab = new(0, b);
                Vector2 ac = new(a, b);
                Vector2 h1 = new Vector2(0.1547, 0.0893) * Width;
                Vector2 h2 = new(h1.X, -h1.Y);
                builder.MoveTo(-a, -b)
                    .LineTo(a, -b)
                    .LineTo(ac)
                    // .ArcTo(a, a, 0, false, false, ab) ->
                    .CurveTo(ac - h1, ab + h2, ab)
                    // .ArcTo(a, a, 0, false, true, aa) ->
                    .CurveTo(ab - h2, aa + h1, aa)
                    .Close();
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (Variants.Contains(Multiple))
                {
                    // Draw multiple paths behind it
                    for (int i = 2; i >= 1; i--)
                    {
                        drawing.BeginTransform(new(new(-i * 2, -i * 2), Matrix2.Identity));
                        drawing.Path(DrawPath);
                        drawing.EndTransform();
                    }
                }
                drawing.Path(DrawPath);
                BoxLabelAnchorPoints.Default.Draw(drawing, this);
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
