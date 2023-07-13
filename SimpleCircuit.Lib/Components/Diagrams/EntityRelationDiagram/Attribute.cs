using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using System.Collections.Generic;
using System;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram
{
    [Drawable("ATTR", "An entity-relationship diagram attribute.", "ERD")]
    public class Attribute : DrawableFactory
    {
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : DiagramBlockInstance, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

            /// <inheritdoc />
            public override string Type => "attribute";

            /// <summary>
            /// Gets or sets the width of the attribute block.
            /// </summary>
            public double Width { get; set; } = 30;

            /// <summary>
            /// Gets or sets the height of the attribute block.
            /// </summary>
            public double Height { get; set; } = 20;

            /// <summary>
            /// Creates a new attribute.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.Ellipse(new(), Width * 0.5, Height * 0.5);
                drawing.Text(Labels[0], new(), new());
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                double a = Width * 0.5;
                double b = Height * 0.5;

                foreach (var pin in pins)
                {
                    if (pin.Orientation.IsZero())
                        pin.Offset = new();
                    else
                    {
                        double x = pin.Orientation.X;
                        double y = pin.Orientation.Y;
                        double iksq = x * x / (a * a) + y * y / (b * b);
                        if (iksq.IsZero())
                        {
                            // This is only possible if the normal is not normalized, don't do anything then
                            pin.Offset = new();
                        }
                        else
                        {
                            // We can extend the orientation to fit
                            iksq = 1.0 / Math.Sqrt(iksq);
                            pin.Offset = pin.Orientation * iksq;
                        }
                    }
                }
            }
        }
    }
}
