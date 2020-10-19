namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A capacitor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("C", "Capacitor", Category = "Analog")]
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
            Pins.Add(Name, new[] { "p", "a" }, "The positive pin.", new Vector2(-5, 0), new Vector2(-1, 0));
            Pins.Add(Name, new[] { "n", "b" }, "The negative pin.", new Vector2(5, 0), new Vector2(1, 0));
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(-5, 0), new Vector2(-1.5, 0),
                new Vector2(1.5, 0), new Vector2(5, 0)
            });
            drawing.Segments(new[]
            {
                new Vector2(-1.5, -4), new Vector2(-1.5, 4),
                new Vector2(1.5, -4), new Vector2(1.5, 4),
            }, "plane");

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
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
