using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An amplifier.
    /// </summary>
    [SimpleKey("INV", "A digital inverter.", Category = "Digital")]
    [SimpleKey("NOT", "An inverter.", Category = "Digital")]
    public class Inverter : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the inverter.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Amplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Inverter(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("input", "The input pin.", this, new(-6, 0), new(-1, 0)), "in", "input");
            Pins.Add(new FixedOrientedPin("output", "The output pin.", this, new(9, 0), new(1, 0)), "out", "output");

            DrawingVariants = Variant.Do(DrawInverter);
        }

        /// <inheritdoc/>
        private void DrawInverter(SvgDrawing drawing)
        {
            drawing.Polygon(new[]
            {
                new Vector2(-6, 6), new Vector2(6, 0), new Vector2(-6, -6)
            });
            drawing.Circle(new Vector2(7.5, 0), 1.5);

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, new Vector2(-2.5, 0), new Vector2());
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Inverter {Name}";
    }
}
