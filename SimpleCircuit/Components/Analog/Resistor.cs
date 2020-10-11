namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A resistor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("R", "Resistor", Category = "Analog")]
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
            Pins.Add(new[] { "p", "pos", "a" }, "The positive pin.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "c", "ctrl" }, "The controlling pin.", new Vector2(0, 8), new Vector2(0, 1));
            Pins.Add(new[] { "n", "neg", "b" }, "The negative pin.", new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            drawing.Polyline(tf.Apply(
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

            if (Pins.IsUsed("c"))
            {
                drawing.Line(tf.Apply(new Vector2(0, 4)), tf.Apply(new Vector2(0, 8)));
                drawing.Polygon(tf.Apply(new[]
                {
                    new Vector2(0, 4), new Vector2(-1, 7), new Vector2(1, 7)
                }));
            }

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(0, -7)), tf.ApplyDirection(new Vector2(0, -1)));
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
