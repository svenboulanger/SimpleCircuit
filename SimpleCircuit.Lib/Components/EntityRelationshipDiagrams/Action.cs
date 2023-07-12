using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.EntityRelationshipDiagrams
{
    [Drawable("ACT", "An entity-relationship diagram action.", "ERD")]
    public class Action : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : LocatedDrawable, ILabeled
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
                Pins.Add(new FixedOrientedPin("left", "The left pin", this, new(), new(-1, 0)), "l", "w", "left");
                Pins.Add(new FixedOrientedPin("top", "The top pin", this, new(), new(0, -1)), "t", "n", "top");
                Pins.Add(new FixedOrientedPin("bottom", "The bottom pin", this, new(), new(0, 1)), "b", "s", "bottom");
                Pins.Add(new FixedOrientedPin("right", "The right pin", this, new(), new(1, 0)), "r", "e", "right");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

                // Reset pin locations
                void SetPinLocation(int index, Vector2 pos) => ((FixedOrientedPin)Pins[index]).Offset = pos;
                SetPinLocation(0, new(-0.5 * Width, 0));
                SetPinLocation(1, new(0, -0.5 * Height));
                SetPinLocation(2, new(0, 0.5 * Height));
                SetPinLocation(3, new(0.5 * Width, 0));
                return true;
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
        }
    }
}
