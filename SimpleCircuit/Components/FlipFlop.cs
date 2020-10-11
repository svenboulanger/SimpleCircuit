namespace SimpleCircuit.Components
{
    /// <summary>
    /// A flip-flop.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("FF", "Flip-flop", Category = "Digital")]
    public class FlipFlop : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlipFlop"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public FlipFlop(string name)
            : base(name)
        {
            Pins.Add(new[] { "d" }, "Data.", new Vector2(-8, 6), new Vector2(-1, 0));
            Pins.Add(new[] { "c", "clk" }, "Clock.", new Vector2(-8, -6), new Vector2(-1, 0));
            Pins.Add(new[] { "nq", "qn" }, "Inverted output.", new Vector2(8, -6), new Vector2(1, 0));
            Pins.Add(new[] { "q" }, "Output", new Vector2(8, 6), new Vector2(1, 0));
        }

        /// <inheritdoc />
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);

            drawing.Polygon(tf.Apply(new[]
            {
                new Vector2(-8, 11), new Vector2(8, 11),
                new Vector2(8, -11), new Vector2(-8, -11)
            }));
            drawing.Polyline(tf.Apply(new[]
            {
                new Vector2(-8, -4), new Vector2(-6, -6), new Vector2(-8, -8)
            }));

            drawing.Text("D", tf.Apply(new Vector2(-7, 5.5)), tf.ApplyDirection(new Vector2(1, 0)), 3, 0.5);
            drawing.Text("C", tf.Apply(new Vector2(-5, -5.5)), tf.ApplyDirection(new Vector2(1, 0)), 3, 0.5);
            drawing.Text("Q", tf.Apply(new Vector2(7, 5.5)), tf.ApplyDirection(new Vector2(-1, 0)), 3, 0.5);
            drawing.Text("NQ", tf.Apply(new Vector2(7, -5.5)), tf.ApplyDirection(new Vector2(-1, 0)), 3, 0.5);
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(10, 8)), tf.ApplyDirection(new Vector2(1, 1)));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Flip-flop {Name}";
    }
}
