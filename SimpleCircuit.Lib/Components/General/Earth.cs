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
        /// <param name="options">Options that can be used for the component.</param>
        public Earth(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, -1)), "a");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            // Wire
            drawing.Line(new(0, 0), new(0, 3), new("wire"));

            // Ground segments
            drawing.Segments(new Vector2[]
            {
                new(-5, 3), new(5, 3),
                new(-5, 3), new(-7, 7),
                new(0, 3), new(-2, 7),
                new(5, 3), new(3, 7)
            });
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Earth {Name}";
    }
}
