namespace SimpleCircuit.Components
{
    /// <summary>
    /// A supply voltage.
    /// </summary>
    /// <seealso cref="IComponent" />
    /// <seealso cref="ITranslating" />
    /// <seealso cref="IRotating" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("POW", "Power plane")]
    public class Power : RotatingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; } = "VDD";

        /// <summary>
        /// Initializes a new instance of the <see cref="Ground"/> class.
        /// </summary>
        public Power(string name)
            : base(name)
        {
            Pins.Add(Name, new[] { "a" }, "The pin.", new Vector2(), new Vector2(0, 1));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Line(new Vector2(0, 0), new Vector2(0, -3));
            drawing.Line(new Vector2(-5, -3), new Vector2(5, -3), "plane");
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Power {Name}";
    }
}
