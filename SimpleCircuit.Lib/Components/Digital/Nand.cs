using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// And gate.
    /// </summary>
    [SimpleKey("NAND", "A 2-input NAND gate.", Category = "Digital")]
    public class Nand : ScaledOrientedDrawable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Or"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Nand(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("a", "The first input.", this, new(-6, -2.5), new(-1, 0)), "a");
            Pins.Add(new FixedOrientedPin("b", "The second input.", this, new(-6, 2.5), new(-1, 0)), "b");
            Pins.Add(new FixedOrientedPin("output", "The output.", this, new(9, 0), new(1, 0)), "o", "output");

            DrawingVariants = Variant.Do(DrawNand);
        }

        /// <inheritdoc />
        private void DrawNand(SvgDrawing drawing)
        {
            drawing.ClosedBezier(new[]
            {
                new Vector2(-6, 5),
                new Vector2(-6, 5), new Vector2(1, 5), new Vector2(1, 5),
                new Vector2(4, 5), new Vector2(6, 2), new Vector2(6, 0),
                new Vector2(6, -2), new Vector2(4, -5), new Vector2(1, -5),
                new Vector2(1, -5), new Vector2(-6, -5), new Vector2(-6, -5)
            });
            drawing.Circle(new Vector2(7.5, 0), 1.5);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Nand {Name}";
    }
}
