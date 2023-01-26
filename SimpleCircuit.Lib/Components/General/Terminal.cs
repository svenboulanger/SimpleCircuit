using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A factory for a terminal.
    /// </summary>
    [Drawable("T", "A common terminal symbol.", "General")]
    public class TerminalFactory : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "terminal";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("p", "The pin.", this, new Vector2(), new Vector2(1, 0)), "p", "a", "o", "i");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 4);

                switch (Variants.Select("input", "output", "inout", "other", "pad", "square"))
                {
                    case 0:
                        // Input
                        drawing.Polygon(new Vector2[]
                        {
                            new(-5, -2), new(-2, -2), new(),
                            new(-2, 2), new(-5, 2)
                        });
                        drawing.Text(Labels[0], new Vector2(-6, 0), new Vector2(-1, 0));
                        break;

                    case 1:
                        // output
                        drawing.Polygon(new Vector2[]
                        {
                            new(-5, 0), new(-3, -2), new(0, -2),
                            new(0, 2), new(-3, 2)
                        });
                        drawing.Text(Labels[0], new Vector2(-6, 0), new Vector2(-1, 0));
                        break;

                    case 2:
                        // inout
                        drawing.Polygon(new Vector2[]
                        {
                            new(-7, 0), new(-5, -2), new(-2, -2), new(),
                            new(-2, 2), new(-5, 2)
                        });
                        drawing.Text(Labels[0], new Vector2(-8, 0), new Vector2(-1, 0));
                        break;

                    case 3:
                        // other
                        drawing.Polygon(new Vector2[]
                        {
                            new(-5, -2), new(0, -2), new(0, 2), new(-5, 2)
                        });
                        drawing.Text(Labels[0], new Vector2(-6, 0), new Vector2(-1, 0));
                        break;

                    case 4:
                        // pad
                        drawing.Rectangle(4, 4, new(-2, 0));
                        drawing.Cross(new(-2, 0), 4);
                        drawing.Text(Labels[0], new Vector2(-5, 0), new Vector2(-1, 0));
                        break;

                    case 5:
                        // square
                        drawing.Rectangle(4, 4, new(-2, 0));
                        drawing.Text(Labels[0], new Vector2(-5, 0), new Vector2(-1, 0));
                        break;

                    default:
                        drawing.Circle(new Vector2(-1.5, 0), 1.5, new("terminal"));
                        drawing.Text(Labels[0], new Vector2(-4, 0), new Vector2(-1, 0));
                        break;
                }
            }
        }
    }
}
