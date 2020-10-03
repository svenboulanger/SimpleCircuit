using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("I")]
    public class CurrentSource : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CurrentSource(string name)
            : base(name)
        {
            Pins.Add(new[] { "p", "b" }, "The current end point.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "n", "a" }, "The current starting point.", new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);

            drawing.Circle(tf.Apply(new Vector2(0, 0)), 6);
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(-3, 0), new Vector2(3, 0),
                new Vector2(3, 0), new Vector2(1, 2),
                new Vector2(3, 0), new Vector2(1, -2),
                new Vector2(6, 0), new Vector2(8, 0)
            }));

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(0, -8)), tf.ApplyDirection(new Vector2(0, -1)));
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
        public override string ToString() => $"Current source {Name}";
    }
}