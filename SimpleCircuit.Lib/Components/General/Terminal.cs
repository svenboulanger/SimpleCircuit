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
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "terminal";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("p", "The pin.", this, new Vector2(), new Vector2(1, 0)), "p", "a", "o", "i");
                DrawingVariants = Variant.Do(DrawTerminal);
            }
            private void DrawTerminal(SvgDrawing drawing)
            {
                drawing.Line(new Vector2(), new Vector2(-4, 0), new("wire"));
                drawing.Circle(new Vector2(-5.5, 0), 1.5, new("terminal"));
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(-8, 0), new Vector2(-1, 0));
            }
        }
    }
}
