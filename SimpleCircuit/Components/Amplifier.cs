namespace SimpleCircuit.Components
{
    /// <summary>
    /// An amplifier.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("A", "Amplifier", Category = "Analog")]
    public class Amplifier : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Amplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Amplifier(string name)
            : base(name)
        {
            Pins.Add(new[] { "in" }, "The input.", new Vector2(-6, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "out" }, "The output.", new Vector2(6, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);

            drawing.Polygon(tf.Apply(new[]
            {
                new Vector2(-6, 6), new Vector2(6, 0), new Vector2(-6, -6)
            }));

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, tf.Apply(new Vector2(-2.5, 0)), new Vector2());
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Amplifier {Name}";
    }
}
