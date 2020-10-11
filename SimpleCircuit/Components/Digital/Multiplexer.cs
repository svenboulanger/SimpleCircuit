namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA)
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("MUX", "Multiplexer", Category = "Digital")]
    public class Multiplexer : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationalTransconductanceAmplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Multiplexer(string name)
            : base(name)
        {
            Pins.Add(new[] { "zero", "a" }, "The '0' input.", new Vector2(-5, -4), new Vector2(-1, 0));
            Pins.Add(new[] { "one", "b" }, "The '1' input.", new Vector2(-5, 4), new Vector2(-1, 0));
            Pins.Add(new[] { "o", "out" }, "The output.", new Vector2(5, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new[] {
                new Vector2(-5, -8),
                new Vector2(5, -4),
                new Vector2(5, 4),
                new Vector2(-5, 8)
            });

            drawing.Text("1", new Vector2(-4, -4), new Vector2(1, 0), 3, 0.5);
            drawing.Text("0", new Vector2(-4, 4), new Vector2(1, 0), 3, 0.5);
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(5, 5), new Vector2(1, 1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"OTA {Name}";
    }
}
