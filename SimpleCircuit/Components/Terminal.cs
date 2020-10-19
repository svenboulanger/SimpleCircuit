namespace SimpleCircuit.Components
{
    /// <summary>
    /// A terminal (input or output).
    /// </summary>
    /// <seealso cref="IComponent" />
    /// <seealso cref="ITranslating" />
    /// <seealso cref="IRotating" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("T", "Terminal"), SimpleKey("P", "Pin")]
    public class Terminal : RotatingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Terminal(string name)
            : base(name)
        {
            Pins.Add(Name, new[] { "p", "a", "o", "i" }, "The pin.", new Vector2(), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Line(new Vector2(), new Vector2(-4, 0));
            drawing.Circle(new Vector2(-5.5, 0), 1.5, "terminal");
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(-10, 0), new Vector2(-1, 0));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Terminal {Name}";
    }
}
