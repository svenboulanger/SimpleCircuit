using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A capacitor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("C")]
    public class Capacitor : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Capacitor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Capacitor(string name)
            : base(name)
        {
            Pins.Add(new[] { "p", "+", "pos", "a" }, new Vector2(-5, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "n", "-", "neg", "b" }, new Vector2(5, 0), new Vector2(1, 0));
        }

        /// <inheritdoc />
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-5, 0), new Vector2(-1.5, 0),
                new Vector2(1.5, 0), new Vector2(5, 0)
            }));
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-1.5, -4), new Vector2(-1.5, 4),
                new Vector2(1.5, -4), new Vector2(1.5, 4),
            }), "plane");

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(0, -7)), tf.ApplyDirection(new Vector2(0, -1)));
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
        public override string ToString() => $"Capacitor {Name}";
    }
}
