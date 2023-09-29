using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.FlowChart
{
    [Drawable("FP", "A Flowchart process.", "Flowchart")]
    public class Process : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : DiagramBlockInstance, ILabeled
        {
            public const string Predefined = "predefined";

            /// <inheritdoc />
            public override string Type => "process";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            [Description("The width of the block.")]
            public double Width { get; set; } = 30.0;

            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            [Description("The height of the block.")]
            public double Height { get; set; } = 15.0;

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.Rectangle(-Width * 0.5, -Height * 0.5, Width, Height);

                if (Variants.Contains(Predefined))
                {
                    double a = Width * 0.5;
                    double b = Height * 0.5;
                    drawing.Line(new(-a + 3, -b), new(-a + 3, b));
                    drawing.Line(new(a - 3, -b), new(a - 3, b));
                }
                Labels.SetDefaultPin(0, location: new(), expand: new());
                Labels.Draw(drawing);
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
                    else if (angle < Math.PI * 0.75)
                        pin.Offset = Interp(new(a, b), new(-a, b), angle - Math.PI * 0.25);
                    else
                        pin.Offset = Interp(new(-a, b), new(-a, -b), angle - Math.PI * 0.75);
                }
            }
        }
    }
}
