namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("I", "Current source", Category = "Analog")]
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
            Pins.Add(Name, new[] { "p", "b" }, "The current end point.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(Name, new[] { "n", "a" }, "The current starting point.", new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Circle(new Vector2(0, 0), 6);
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(-3, 0), new Vector2(3, 0),
                new Vector2(3, 0), new Vector2(1, 2),
                new Vector2(3, 0), new Vector2(1, -2),
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
        public override string ToString() => $"Current source {Name}";
    }
}