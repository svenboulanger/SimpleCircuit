namespace SimpleCircuit.Components
{
    /// <summary>
    /// A PMOS transistor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("MP", "PMOS transistor", Category = "Analog"), SimpleKey("PMOS", "PMOS transistor", Category = "Analog")]
    public class Pmos : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pmos"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Pmos(string name)
            : base(name)
        {
            Pins.Add(new[] { "d", "drain" }, "The drain.", new Vector2(8, 0), new Vector2(1, 0));
            Pins.Add(new[] { "g", "gate" }, "The gate.", new Vector2(0, 11), new Vector2(0, 1));
            Pins.Add(new[] { "b", "bulk" }, "The bulk.", new Vector2(0, 0), new Vector2(0, -1));
            Pins.Add(new[] { "s", "source" }, "The source.", new Vector2(-8, 0), new Vector2(-1, 0));
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(0, 11), new Vector2(0, 9),
                new Vector2(-6, 6), new Vector2(6, 6),
                new Vector2(-6, 4), new Vector2(6, 4)
            }));
            drawing.Circle(tf.Apply(new Vector2(0, 7.5)), 1.5);

            drawing.Polyline(tf.Apply(new[]
            {
                new Vector2(-8, 0), new Vector2(-4, 0), new Vector2(-4, 4)
            }));
            drawing.Polyline(tf.Apply(new[]
            {
                new Vector2(8, 0), new Vector2(4, 0), new Vector2(4, 4)
            }));

            if (Pins.IsUsed("b"))
                drawing.Line(tf.Apply(new Vector2(0, 4)), tf.Apply(new Vector2(0, 0)));

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, tf.Apply(new Vector2(1, -3)), tf.ApplyDirection(new Vector2(1, -1)));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"PMOS {Name}";
    }
}
