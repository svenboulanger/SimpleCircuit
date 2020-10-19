namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A PMOS transistor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("MP", "PMOS transistor", Category = "Analog"), SimpleKey("PMOS", "PMOS transistor", Category = "Analog")]
    public class Pmos : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pmos"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Pmos(string name)
            : base(name)
        {
            Pins.Add(Name, new[] { "d", "drain" }, "The drain.", new Vector2(8, 0), new Vector2(1, 0));
            Pins.Add(Name, new[] { "g", "gate" }, "The gate.", new Vector2(0, 11), new Vector2(0, 1));
            Pins.Add(Name, new[] { "b", "bulk" }, "The bulk.", new Vector2(0, 0), new Vector2(0, -1));
            Pins.Add(Name, new[] { "s", "source" }, "The source.", new Vector2(-8, 0), new Vector2(-1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(0, 11), new Vector2(0, 9),
                new Vector2(-6, 6), new Vector2(6, 6),
                new Vector2(-6, 4), new Vector2(6, 4)
            });
            drawing.Circle(new Vector2(0, 7.5), 1.5);

            drawing.Polyline(new[] { new Vector2(-8, 0), new Vector2(-4, 0), new Vector2(-4, 4) });
            drawing.Polyline(new[] { new Vector2(8, 0), new Vector2(4, 0), new Vector2(4, 4) });

            if (Pins.IsUsed("b"))
                drawing.Line(new Vector2(0, 4), new Vector2(0, 0));

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, new Vector2(1, -3), new Vector2(1, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"PMOS {Name}";
    }
}
