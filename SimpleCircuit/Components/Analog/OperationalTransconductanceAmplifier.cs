namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA)
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("OTA", "Operational transconductance amplifier", Category = "Analog")]
    public class OperationalTransconductanceAmplifier : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationalTransconductanceAmplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public OperationalTransconductanceAmplifier(string name)
            : base(name)
        {
            Pins.Add(Name, new[] { "n" }, "The negative input.", new Vector2(-5, -4), new Vector2(-1, 0));
            Pins.Add(Name, new[] { "p" }, "The positive input.", new Vector2(-5, 4), new Vector2(-1, 0));
            Pins.Add(Name, new[] { "o", "out" }, "The output.", new Vector2(5, 0), new Vector2(1, 0));
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
            drawing.Segments(new[]
            {
                new Vector2(-3, -4), new Vector2(-1, -4),
                new Vector2(-2, 5), new Vector2(-2, 3),
                new Vector2(-3, 4), new Vector2(-1, 4)
            });

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
