using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A resistor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("R")]
    public class Resistor : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Resistor(string name)
            : base(name)
        {
            Pins.Add(new[] { "p", "+", "pos", "a" }, new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "n", "-", "neg", "b" }, new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            drawing.Poly(tf.Apply(
                new Vector2[]
                {
                    new Vector2(-8, 0),
                    new Vector2(-6, 0),
                    new Vector2(-5, -4),
                    new Vector2(-3, 4),
                    new Vector2(-1, -4),
                    new Vector2(1, 4),
                    new Vector2(3, -4),
                    new Vector2(5, 4),
                    new Vector2(6, 0),
                    new Vector2(8, 0)
                }));

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
        public override string ToString() => $"Resistor {Name}";

    }
}
