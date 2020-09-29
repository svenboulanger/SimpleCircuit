using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// An operational amplifier.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("OA")]
    public class Opamp : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Opamp"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Opamp(string name)
            : base(name)
        {
            Pins.Add(new[] { "-", "n" }, new Vector2(-8, -4), new Vector2(-1, 0));
            Pins.Add(new[] { "+", "p" }, new Vector2(-8, 4), new Vector2(-1, 0));
            Pins.Add(new[] { "o", "out" }, new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            drawing.Poly(tf.Apply(new[] {
                new Vector2(-8, -8),
                new Vector2(8, 0),
                new Vector2(-8, 8),
                new Vector2(-8, -8)
            }));
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-6, -4), new Vector2(-4, -4),
                new Vector2(-5, 5), new Vector2(-5, 3),
                new Vector2(-6, 4), new Vector2(-4, 4)
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
        public override string ToString() => $"Opamp {Name}";
    }
}
