namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("V", "Voltage source", Category = "Analog")]
    public class VoltageSource : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public VoltageSource(string name)
            : base(name)
        {
            Pins.Add(Name, new[] { "n", "neg" }, "The positive pin.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(Name, new[] { "p", "pos" }, "The negative pin.", new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Circle(new Vector2(0, 0), 6);
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(-3, -1), new Vector2(-3, 1),
                new Vector2(3, -1), new Vector2(3, 1),
                new Vector2(2, 0), new Vector2(4, 0),
                new Vector2(6, 0), new Vector2(8, 0)
            });

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -8), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Voltage source {Name}";
    }
}