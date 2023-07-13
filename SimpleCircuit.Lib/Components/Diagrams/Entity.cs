using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Components.Diagrams
{
    [Drawable("ENT", "An entity-relationship diagram entity.", "ERD")]
    public class Entity : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : LocatedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new Labels(20);

            /// <inheritdoc />
            public override string Type => "entity";

            /// <summary>
            /// Gets or sets the width of the entity block.
            /// </summary>
            [Description("The width of the entity block.")]
            public double Width { get; set; } = 30;

            [Description("The height of a line for attributes.")]
            public double LineHeight { get; set; } = 8;

            /// <summary>
            /// Gets the height of the entity block (only valid after <see cref="Reset(IDiagnosticHandler)"/>).
            /// </summary>
            protected double Height { get; private set; }

            /// <summary>
            /// Creates a new entity.
            /// </summary>
            /// <param name="name">The name of the entity.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

                Pins.Clear();
                double w = Width * 0.5;
                if (Labels.Count <= 1)
                {
                    Height = LineHeight * 2;
                    Pins.Add(new FixedOrientedPin("left", "The left pin", this, new(-w, 0), new(-1, 0)), "l", "w", "left");
                    Pins.Add(new FixedOrientedPin("top", "The top pin", this, new(0, -LineHeight), new(0, -1)), "t", "n", "top");
                    Pins.Add(new FixedOrientedPin("bottom", "The bottom pin", this, new(0, LineHeight), new(0, 1)), "b", "s", "bottom");
                    Pins.Add(new FixedOrientedPin("right", "The right pin", this, new(w, 0), new(1, 0)), "r", "e", "right");
                }
                else
                {
                    // We have a header and attributes
                    Height = LineHeight * Labels.Count;
                    Pins.Add(new FixedOrientedPin("left", "The left pin", this, new(-w, 0), new(-1, 0)), "l", "w", "left");
                    Pins.Add(new FixedOrientedPin("top", "The top pin", this, new(0, -LineHeight * 0.5), new(0, -1)), "t", "n", "top");
                    Pins.Add(new FixedOrientedPin("bottom", "The bottom pin", this, new(0, -LineHeight * 0.5 + Height), new(0, 1)), "b", "s", "bottom");

                    // Add pins for all the attributes
                    for (int i = 1; i < Labels.Count; i++)
                    {
                        Pins.Add(new FixedOrientedPin($"attribute {i} left", $"The left pin for attribute {i}.", this, new(-w, i * LineHeight), new(-1, 0)), $"l{i}", $"w{i}", $"left{i}");
                        Pins.Add(new FixedOrientedPin($"attribute {i} right", $"The right pin of attribute {i}.", this, new(w, i * LineHeight), new(1, 0)), $"r{i}", $"e{i}", $"right{i}");
                    }
                    Pins.Add(new FixedOrientedPin("right", "The right pin", this, new(w, 0), new(1, 0)), "r", "e", "right");
                }

                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (Labels.Count <= 1)
                {
                    drawing.Rectangle(Width, Height, new(), new("erd"));
                    drawing.Text(Labels[0], new(), new(), new("header"));
                }
                else
                {
                    double w = Width * 0.5;
                    drawing.Rectangle(Width, Height, new(0, (Height - LineHeight) * 0.5));
                    drawing.Line(new(-w, LineHeight * 0.5), new(w, LineHeight * 0.5));

                    drawing.Text(Labels[0], new(), new(), new("header"));
                    for (int i = 1; i < Labels.Count; i++)
                        drawing.Text(Labels[i], new(-w + 2.0, i * LineHeight), new(1, 0), new("attribute"));
                }
            }
        }
    }
}
