using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA).
    /// </summary>
    [SimpleKey("MUX", "A 2-input multiplexer.", Category = "Digital")]
    public class Multiplexer : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the MUX.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationalTransconductanceAmplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Multiplexer(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("a", "The '0' input.", this, new(-5, -4), new(-1, 0)), "a", "0");
            Pins.Add(new FixedOrientedPin("b", "The '1' input.", this, new(-5, 4), new(-1, 0)), "b", "1");
            Pins.Add(new FixedOrientedPin("c", "The controlling input.", this, new(0, -6), new(0, -1)), "c");
            Pins.Add(new FixedOrientedPin("output", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "output");

            DrawingVariants = Variant.Do(DrawMultiplexer);
        }

        /// <inheritdoc/>
        private void DrawMultiplexer(SvgDrawing drawing)
        {
            drawing.Polygon(new[] {
                new Vector2(-5, -8),
                new Vector2(5, -4),
                new Vector2(5, 4),
                new Vector2(-5, 8)
            });

            drawing.Text("1", new Vector2(-4, -4), new Vector2(1, 0), new("small"));
            drawing.Text("0", new Vector2(-4, 4), new Vector2(1, 0), new("small"));
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(5, 5), new Vector2(1, 1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"MUX {Name}";
    }
}
