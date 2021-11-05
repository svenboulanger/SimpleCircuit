using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// And gate.
    /// </summary>
    [SimpleKey("OR", "A 2-input OR gate.", Category = "Digital")]
    public class Or : ScaledOrientedDrawable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Or"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Or(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("a", "The first input.", this, new(-4, -2.5), new(-1, 0)), "a");
            Pins.Add(new FixedOrientedPin("b", "The second input.", this, new(-4, 2.5), new(-1, 0)), "b");
            Pins.Add(new FixedOrientedPin("output", "The output.", this, new(6, 0), new(1, 0)), "o", "out", "output");
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.ClosedBezier(new[]
            {
                new Vector2(-5, 5),
                new Vector2(-5, 5), new Vector2(-4, 5), new Vector2(-4, 5),
                new Vector2(1, 5), new Vector2(4, 3), new Vector2(6, 0),
                new Vector2(4, -3), new Vector2(1, -5), new Vector2(-4, -5),
                new Vector2(-4, -5), new Vector2(-3, -5), new Vector2(-5, -5),
                new Vector2(-3, -2), new Vector2(-3, 2), new Vector2(-5, 5)
            });
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Or {Name}";
    }
}
