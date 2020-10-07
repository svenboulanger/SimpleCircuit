namespace SimpleCircuit.Components
{
    /// <summary>
    /// An NMOS transistor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("MN", "NMOS transistor"), SimpleKey("NMOS", "NMOS transistor")]
    public class Nmos : TransformingComponent, ILabeled
    {
        /// <inheritdoc />
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the bulk contact should be rendered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the bulk should be rendered; otherwise, <c>false</c>.
        /// </value>
        public bool ShowBulk { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nmos"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Nmos(string name)
            : base(name)
        {
            Pins.Add(new[] { "s", "source" }, "The source.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "g", "gate" }, "The gate.", new Vector2(0, 8), new Vector2(0, 1));
            Pins.Add(new[] { "b", "bulk" }, "The bulk.", new Vector2(0, 0), new Vector2(0, -1));
            Pins.Add(new[] { "d", "drain" }, "The drain.", new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(0, 8), new Vector2(0, 6),
                new Vector2(-6, 6), new Vector2(6, 6),
                new Vector2(-6, 4), new Vector2(6, 4)
            }));

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
                drawing.Text(Label, tf.Apply(new Vector2(-1, -3)), tf.ApplyDirection(new Vector2(-1, -1)));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"NMOS {Name}";
    }
}
