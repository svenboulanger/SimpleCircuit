namespace SimpleCircuit.Components
{
    /// <summary>
    /// A ground terminal.
    /// </summary>
    /// <seealso cref="Component" />
    [SimpleKey("GND", "Ground")]
    public class Ground : RotatingComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ground"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Ground(string name)
            : base(name)
        {
            Pins.Add(new[] { "a" }, "The pin.", new Vector2(), new Vector2(0, -1));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(0, 0), new Vector2(0, 3),
                new Vector2(-5, 3), new Vector2(5, 3),
                new Vector2(-3, 5), new Vector2(3, 5),
                new Vector2(-1, 7), new Vector2(1, 7)
            });
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Ground {Name}";
    }
}
