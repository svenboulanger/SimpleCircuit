namespace SimpleCircuit.Components.General
{
    /// <summary>
    /// A direction that is like a regular point, but can be oriented.
    /// This is useful for example when combined with subcircuits to give an orientation.
    /// </summary>
    [SimpleKey("DIR", "Direction")]
    public class Direction : RotatingComponent
    {
        /// <summary>
        /// Initializers a new instance of the <see cref="Direction"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Direction(string name)
            : base(name)
        {
            Pins.Add(new[] { "in" }, "The entry point.", new Vector2(), new Vector2(-1, 0));
            Pins.Add(new[] { "out" }, "The output.", new Vector2(), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
        }

        /// <summary>
        /// Converts to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"Direction {Name}";
    }
}
