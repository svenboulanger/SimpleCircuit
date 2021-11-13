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
        /// <param name="options">Options that can be used for the component.</param>
        public Ground(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("p", "The one and only pin.", this, new(0, 0), new(0, -1)), "a", "p");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            // Wire
            drawing.Line(new(0, 0), new(0, 3), new("wire"));

            // Ground
            drawing.Segments(new Vector2[]
            {
                new(-5, 3), new(5, 3),
                new(-3, 5), new(3, 5),
                new(-1, 7), new(1, 7)
            });
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Ground {Name}";
    }
}
