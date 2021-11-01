using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A ground terminal.
    /// </summary>
    [SimpleKey("GND", "A common ground symbol.", Category = "General")]
    public class Ground : ScaledOrientedDrawable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ground"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Ground(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("p", "The one and only pin.", this, new(0, 0), new(0, -1)), "a", "p");
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
