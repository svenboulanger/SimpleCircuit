using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA)
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("OTA")]
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
            Pins.Add(new[] { "n" }, "The negative input.", new Vector2(-5, -4), new Vector2(-1, 0));
            Pins.Add(new[] { "p" }, "The positive input.", new Vector2(-5, 4), new Vector2(-1, 0));
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
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-3, -4), new Vector2(-1, -4),
                new Vector2(-2, 5), new Vector2(-2, 3),
                new Vector2(-3, 4), new Vector2(-1, 4)
            }));

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(5, 5)), tf.ApplyDirection(new Vector2(1, 1)));
        }

        /// <inheritdoc/>
        public override void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(X) + new Squared(Y);
            minimizer.AddConstraint(new Squared(Scale) - 1);
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
