namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A diode.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("D", "Diode", Category = "Analog")]
    public class Diode : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Diode"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Diode(string name)
            : base(name)
        {
            Pins.Add(new[] { "p", "a" }, "The anode.", new Vector2(-6, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "n", "b", "c" }, "The cathode.", new Vector2(6, 0), new Vector2(1, 0));
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(-6, 0), new Vector2(-4, 0),
                new Vector2(4, -4), new Vector2(4, 4),
                new Vector2(4, 0), new Vector2(6, 0)
            });
            drawing.Polygon(new[] {
                new Vector2(-4, -4),
                new Vector2(4, 0),
                new Vector2(-4, 4),
                new Vector2(-4, 4)
            });

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Diode {Name}";
    }
}
