using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A flip-flop.
    /// </summary>
    [SimpleKey("FF", "A general flip-flop.", Category = "Digital")]
    public class FlipFlop : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the transistor.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlipFlop"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public FlipFlop(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("data", "The data pin.", this, new(-8, -6), new(-1, 0)), "d", "data");
            Pins.Add(new FixedOrientedPin("clock", "The clock pin.", this, new(-8, 6), new(-1, 0)), "c", "clock");
            Pins.Add(new FixedOrientedPin("reset", "The reset pin.", this, new(0, 11), new(0, 1)), "r", "rst", "reset");
            Pins.Add(new FixedOrientedPin("set", "The set pin.", this, new(0, -11), new(0, -1)), "s", "set");
            Pins.Add(new FixedOrientedPin("nq", "The inverted output pin.", this, new(8, 6), new(1, 0)), "nq", "qn");
            Pins.Add(new FixedOrientedPin("q", "The output pin.", this, new(8, -6), new(1, 0)), "q");

            DrawingVariants = Variant.Do(DrawFlipFlop);
        }

        /// <inheritdoc />
        private void DrawFlipFlop(SvgDrawing drawing)
        {
            drawing.Polygon(new[]
            {
                new Vector2(-8, 11), new Vector2(8, 11),
                new Vector2(8, -11), new Vector2(-8, -11)
            });
            drawing.Polyline(new[]
            {
                new Vector2(-8, 4), new Vector2(-6, 6), new Vector2(-8, 8)
            });
            drawing.Text("D", new Vector2(-7, -5.5), new Vector2(1, 0));
            drawing.Text("C", new Vector2(-5, 5.5), new Vector2(1, 0));
            drawing.Text("Q", new Vector2(7, -5.5), new Vector2(-1, 0));
            if (Pins["nq"].Connections > 0)
                drawing.Text("NQ", new Vector2(7, 5.5), new Vector2(-1, 0));
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(10, -8), new Vector2(1, 1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Flip-flop {Name}";
    }
}
