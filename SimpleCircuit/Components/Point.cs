namespace SimpleCircuit.Components
{
    /// <summary>
    /// A point that can serve as an inflection point or a node for multiple wires.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("X", "Point")]
    public class Point : TranslatingComponent
    {
        /// <summary>
        /// Gets or sets the number of wires that are connected to this point.
        /// </summary>
        /// <value>
        /// The wires.
        /// </value>
        public int Wires { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Point(string name)
            : base(name)
        {
            Pins.Add(new[] { "a", "p" }, "The point.", new Vector2(), new Vector2(0, -1));
            Wires = 0;
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            // If there are more than 2 wires, then let's draw a point
            if (Wires > 2)
                drawing.Circle(new Vector2(), 1);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Point {Name}";
    }
}
