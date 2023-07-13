using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams
{
    [Drawable("ACT", "An entity-relationship diagram action.", "ERD")]
    public class Action : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : DiagramBlockInstance, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

            /// <inheritdoc />
            public override string Type => "action";

            /// <summary>
            /// Gets or sets the width of the action block.
            /// </summary>
            public double Width { get; set; } = 40;

            /// <summary>
            /// Gets or sets the height of the action block.
            /// </summary>
            public double Height { get; set; } = 20;

            /// <summary>
            /// Creates a new action.
            /// </summary>
            /// <param name="name">The name of the action.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.Path(builder =>
                {
                    builder.MoveTo(-Width * 0.5, 0)
                        .LineTo(0, -Height * 0.5)
                        .LineTo(Width * 0.5, 0)
                        .LineTo(0, Height * 0.5)
                        .Close();
                });

                drawing.Text(Labels[0], new(), new());
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                double a = 0.5 * Width;
                double b = 0.5 * Height;

                foreach (var pin in pins)
                {
                    double x = pin.Orientation.X;
                    double y = pin.Orientation.Y;
                    double k = 0.0;
                    if (x.IsZero())
                    {
                        if (y.IsZero())
                            pin.Offset = new();
                        else
                            k = b;
                    }
                    else if (x < 0)
                    {
                        if (y < 0)
                            k = 1.0 / (-x / a - y / b);
                        else
                            k = 1.0 / (-x / a + y / b);
                    }
                    else
                    {
                        if (y < 0)
                            k = 1.0 / (x / a - y / b);
                        else
                            k = 1.0 / (x / a + y / b);
                    }
                    if (k.IsZero())
                        pin.Offset = new();
                    else
                        pin.Offset = pin.Orientation * k;
                }
            }
        }
    }
}
