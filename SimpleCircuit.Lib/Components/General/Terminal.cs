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

                switch (Variants.Select("input", "in", "output", "out", "inout", "other", "pad", "square", "none"))
                {
                    case 0:
                    case 1:
                        // Input
                        drawing.Polygon(new Vector2[]
                        {
                            new(-5, -2), new(-2, -2), new(),
                            new(-2, 2), new(-5, 2)
                        });
                        Labels.SetDefaultPin(0, location: new(-6, 0), expand: new(-1, 0));
                        break;

                    case 2:
                    case 3:
                        // output
                        drawing.Polygon(new Vector2[]
                        {
                            new(-5, 0), new(-3, -2), new(0, -2),
                            new(0, 2), new(-3, 2)
                        });
                        Labels.SetDefaultPin(0, location: new(-6, 0), expand: new(-1, 0));
                        break;

                    case 4:
                        // inout
                        drawing.Polygon(new Vector2[]
                        {
                            new(-7, 0), new(-5, -2), new(-2, -2), new(),
                            new(-2, 2), new(-5, 2)
                        });
                        Labels.SetDefaultPin(0, location: new(-8, 0), expand: new(-1, 0));
                        break;

                    case 5:
                        // other
                        drawing.Polygon(new Vector2[]
                        {
                            new(-5, -2), new(0, -2), new(0, 2), new(-5, 2)
                        });
                        Labels.SetDefaultPin(0, location: new(-6, 0), expand: new(-1, 0));
                        break;

                    case 6:
                        // pad
                        drawing.Rectangle(-4, -2, 4, 4);
                        drawing.Cross(new(-2, 0), 4);
                        Labels.SetDefaultPin(0, location: new(-5, 0), expand: new(-1, 0));
                        break;

                    case 7:
                        // square
                        drawing.Rectangle(-4, -2, 4, 4);
                        Labels.SetDefaultPin(0, location: new(-5, 0), expand: new(-1, 0));
                        break;

                    case 8:
                        // None
                        Labels.SetDefaultPin(0, location: new(-1, 0), expand: new(-1, 0));
                        break;

                    default:
                        drawing.Circle(new Vector2(-1.5, 0), 1.5, new("terminal"));
                        Labels.SetDefaultPin(0, location: new(-4, 0), expand: new(-1, 0));
                        break;
                }
                Labels.Draw(drawing);
            }
        }
    }
}
