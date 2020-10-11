namespace SimpleCircuit.Components
{
    /// <summary>
    /// Single-pole double throw switch.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("SPDT", "Single-pole double throw switch", Category = "Analog")]
    public class SwitchSinglePoleDoubleThrow : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchSinglePoleDoubleThrow"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SwitchSinglePoleDoubleThrow(string name)
            : base(name)
        {
            Pins.Add(new[] { "p" }, "The pole pin.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "c", "ctrl" }, "The controlling pin.", new Vector2(0, 6), new Vector2(0, 1));
            Pins.Add(new[] { "t1" }, "The first throwing pin.", new Vector2(8, 4), new Vector2(1, 0));
            Pins.Add(new[] { "t2" }, "The second throwing pin.", new Vector2(8, -4), new Vector2(1, 0));
        }

        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);

            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 4), new Vector2(8, 4),
                new Vector2(6, -4), new Vector2(8, -4)
            }));
            drawing.Circle(tf.Apply(new Vector2(-5, 0)), 1);
            drawing.Circle(tf.Apply(new Vector2(5, 4)), 1);
            drawing.Circle(tf.Apply(new Vector2(5, -4)), 1);
            drawing.Line(tf.Apply(new Vector2(-4, 0)), tf.Apply(new Vector2(4, 4)));

            if (Pins.IsUsed("c"))
                drawing.Line(tf.Apply(new Vector2(0, 2)), tf.Apply(new Vector2(0, 6)));
        }
    }
}
