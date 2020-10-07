namespace SimpleCircuit.Components
{
    /// <summary>
    /// A diode.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("D", "Diode")]
    public class Diode : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Diode"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Diode(string name)
            : base(name)
        {
            Pins.Add(new[] { "p", "a" }, "The anode.", new Vector2(-6, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "n", "b", "c" }, "The cathode.", new Vector2(6, 0), new Vector2(1, 0));
        }

        /// <inheritdoc />
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);

            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-6, 0), new Vector2(-4, 0),
                new Vector2(4, -4), new Vector2(4, 4),
                new Vector2(4, 0), new Vector2(6, 0)
            }));
            drawing.Polygon(tf.Apply(new[] {
                new Vector2(-4, -4),
                new Vector2(4, 0),
                new Vector2(-4, 4),
                new Vector2(-4, 4)
            }));

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(0, -6)), tf.ApplyDirection(new Vector2(0, -1)));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Diode {Name}";
    }
}
