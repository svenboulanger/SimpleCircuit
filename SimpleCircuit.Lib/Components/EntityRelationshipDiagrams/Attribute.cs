using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.EntityRelationshipDiagrams
{
    [Drawable("ATTR", "An entity-relationship diagram attribute.", "ERD")]
    public class Attribute : DrawableFactory
    {
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : LocatedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

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
                drawing.Ellipse(new(), Width * 0.5, Height * 0.5);
                drawing.Text(Labels[0], new(), new());
            }
        }
    }
}
