using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A terminal (input or output).
    /// </summary>
    [SimpleKey("T", "A common terminal symbol.", Category = "General")]
    public class Terminal : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the terminal.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Terminal(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("p", "The pin.", this, new Vector2(), new Vector2(1, 0)), "p", "a", "o", "i");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            // Wire
            drawing.Line(new Vector2(), new Vector2(-4, 0), new("wire"));

            // Terminal
            drawing.Circle(new Vector2(-5.5, 0), 1.5, new("terminal"));

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(-10, 0), new Vector2(-1, 0));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Terminal {Name}";
    }
}
