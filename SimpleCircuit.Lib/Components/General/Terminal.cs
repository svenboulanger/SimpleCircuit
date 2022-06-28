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
            public string Label { get; set; }

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
                drawing.Circle(new Vector2(-1.5, 0), 1.5, new("terminal"));
                drawing.Text(Label, new Vector2(-4, 0), new Vector2(-1, 0));
            }
        }
    }
}
