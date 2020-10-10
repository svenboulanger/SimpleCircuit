namespace SimpleCircuit.Components
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA)
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("MUX", "Multiplexer")]
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
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            drawing.Polygon(tf.Apply(new[] {
                new Vector2(-5, -8),
                new Vector2(5, -4),
                new Vector2(5, 4),
                new Vector2(-5, 8)
            }));

            drawing.Text("1", tf.Apply(new Vector2(-4, -4)), tf.ApplyDirection(new Vector2(1, 0)), 3, 0.5);
            drawing.Text("0", tf.Apply(new Vector2(-4, 4)), tf.ApplyDirection(new Vector2(1, 0)), 3, 0.5);
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(5, 5)), tf.ApplyDirection(new Vector2(1, 1)));
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
