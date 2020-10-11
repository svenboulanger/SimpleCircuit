namespace SimpleCircuit.Components
{
    /// <summary>
    /// And gate.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    [SimpleKey("XOR", "Xor gate.", Category = "Digital")]
    public class Xor : TransformingComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Or"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Xor(string name)
            : base(name)
        {
            Pins.Add(new[] { "a", "in1" }, "First input.", new Vector2(-5.31, 2), new Vector2(-1, 0));
            Pins.Add(new[] { "b", "in2" }, "Second input.", new Vector2(-5.31, -2), new Vector2(-1, 0));
            Pins.Add(new[] { "o", "out" }, "Output.", new Vector2(6, 0), new Vector2(1, 0));
        }

        /// <inheritdoc />
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            drawing.ClosedBezier(tf.Apply(new[]
            {
                new Vector2(-5, 5),
                new Vector2(-5, 5), new Vector2(-4, 5), new Vector2(-4, 5),
                new Vector2(1, 5), new Vector2(4, 3), new Vector2(6, 0),
                new Vector2(4, -3), new Vector2(1, -5), new Vector2(-4, -5),
                new Vector2(-4, -5), new Vector2(-3, -5), new Vector2(-5, -5),
                new Vector2(-3, -2), new Vector2(-3, 2), new Vector2(-5, 5)
            }));
            drawing.OpenBezier(tf.Apply(new[]
            {
                new Vector2(-6.5, -5), new Vector2(-4.5, -2), new Vector2(-4.5, 2), new Vector2(-6.5, 5)
            }));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Or {Name}";
    }
}
