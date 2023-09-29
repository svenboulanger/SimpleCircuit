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
            [Description("The width of the block.")]
            public double Width { get; set; } = 30;

            /// <summary>
            /// Gets or sets the height of the attribute block.
            /// </summary>
            [Description("The height of the block.")]
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
                Labels.SetDefaultPin(0, location: new(), expand: new());
                Labels.Draw(drawing);
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
                        double nx = pin.Orientation.X;
                        double ny = pin.Orientation.Y;
                        double k = 1.0 / Math.Sqrt(nx * nx / b / b + ny * ny / a / a);

                        pin.Offset = new(
                            a * nx * k / b,
                            b * ny * k / a);
                    }
                }
            }
        }
    }
}
