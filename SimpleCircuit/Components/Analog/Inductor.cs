namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An inductor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("L", "Inductor", Category = "Analog")]
    public class Inductor : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inductor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Inductor(string name)
            : base(name)
        {
            Pins.Add(new[] { "p", "a" }, "The positive pin.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "n", "b" }, "The negative pin.", new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 0), new Vector2(8, 0)
            });
            drawing.SmoothBezier(new[]
            {
                new Vector2(-6, 0),
                new Vector2(-6, -4), new Vector2(-2, -4), new Vector2(-2, 0),
                new Vector2(-4, 4), new Vector2(-4, 0),
                new Vector2(1, -4), new Vector2(1, 0),
                new Vector2(-1, 4), new Vector2(-1, 0),
                new Vector2(4, -4), new Vector2(4, 0),
                new Vector2(2, 4), new Vector2(2, 0),
                new Vector2(6, -4), new Vector2(6, 0)
            });

            drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Inductor {Name}";
    }
}
