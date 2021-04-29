namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A switch.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("S", "Voltage switch", Category = "Analog")]
    public class Switch : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Switch"/> is closed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if closed; otherwise, <c>false</c>.
        /// </value>
        public double Closed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Switch"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Switch(string name)
            : base(name)
        {
            Pins.Add(new[] { "p", "a" }, "The positive pin.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "c", "ctrl" }, "The controlling pin.", new Vector2(0, 6), new Vector2(0, 1));
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
            drawing.Circle(new Vector2(-5, 0), 1);
            drawing.Circle(new Vector2(5, 0), 1);

            if (!Closed.IsZero())
                drawing.Line(new Vector2(-4, 0), new Vector2(4, 0));
            else
                drawing.Line(new Vector2(-4, 0), new Vector2(4, 4));

            if (Pins.IsUsed("c"))
            {
                if (!Closed.IsZero())
                    drawing.Line(new Vector2(0, 0), new Vector2(0, 6));
                else
                    drawing.Line(new Vector2(0, 2), new Vector2(0, 6));
            }

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Switch {Name}";
    }
}
