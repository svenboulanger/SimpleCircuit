using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// An earth terminal.
    /// </summary>
    [SimpleKey("SGND", "Signal ground symbol.", Category = "General")]
    public class SignalGround : ScaledOrientedDrawable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Earth"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public SignalGround(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, -1)), "a");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Line(new Vector2(0, 0), new Vector2(0, 3));
            drawing.Polygon(new[]
            {
                new Vector2(-5, 3), new Vector2(5, 3), new Vector2(0, 7)
            });
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Signal ground {Name}";
    }
}
