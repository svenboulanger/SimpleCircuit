using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A resistor.
    /// </summary>
    [SimpleKey("R", "A resistor. Potentially programmable.", Category = "Analog")]
    public class Resistor : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the resistor.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Resistor(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("p", "The positive pin.", this, new(-8, 0), new(-1, 0)), "p", "pos", "a");
            Pins.Add(new FixedOrientedPin("ctrl", "The controlling pin.", this, new(0, 8), new(0, 1)), "c", "ctrl");
            Pins.Add(new FixedOrientedPin("n", "The negative pin.", this, new(8, 0), new(1, 0)), "n", "neg", "b");

            DrawingVariants = Variant.All(
                Variant.Do(DrawResistor),
                Variant.If("programmable").Do(DrawProgrammable));
        }

        private void DrawResistor(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new Vector2[]
            {
                new(-8, 0), new(-6, 0),
                new(6, 0), new(8, 0)
            }, new("wire"));

            // The resistor
            drawing.Polyline(new Vector2[]
            {
                new Vector2(-6, 0),
                new Vector2(-5, -4),
                new Vector2(-3, 4),
                new Vector2(-1, -4),
                new Vector2(1, 4),
                new Vector2(3, -4),
                new Vector2(5, 4),
                new Vector2(6, 0)
            });

            // Controlled resistor
            if (Pins["c"].Connections > 0)
                drawing.Line(new Vector2(0, 8), new Vector2(0, 4), new("wire") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
        }
        private void DrawProgrammable(SvgDrawing drawing)
            => drawing.Line(new(-5, 5), new(6, -7), new("arrow") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Resistor {Name}";

    }
}
