using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// An earth terminal.
    /// </summary>
    [SimpleKey("EARTH", "Represents an earth connection.", Category = "General")]
    public class Earth : ScaledOrientedDrawable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Earth"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Earth(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, -1)), "a");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(0, 0), new Vector2(0, 3),
                new Vector2(-5, 3), new Vector2(5, 3),
                new Vector2(-5, 3), new Vector2(-7, 7),
                new Vector2(0, 3), new Vector2(-2, 7),
                new Vector2(5, 3), new Vector2(3, 7)
            });
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Earth {Name}";
    }
}
