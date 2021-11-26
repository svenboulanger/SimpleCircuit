using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An impedance/admittance.
    /// </summary>
    [SimpleKey("Z", "An impedance.", Category = "Analog")]
    [SimpleKey("Y", "An admittance.", Category = "Analog")]
    public class Impedance : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the element.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Impedance"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Impedance(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-8, 0), new(-1, 0)), "p", "pos", "a");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(8, 0), new(1, 0)), "n", "neg", "b");

            DrawingVariants = Variant.All(
                Variant.Do(DrawImpedance),
                Variant.If("programmable").Do(DrawProgrammable));
        }

        /// <inheritdoc/>
        private void DrawImpedance(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 0), new Vector2(8, 0)
            }, new("wire"));

            // The rectangle
            drawing.Polygon(new Vector2[] { new(-6, 3), new(6, 3), new(6, -3), new(-6, -3) });

            // Depending on the orientation, let's anchor the text differently
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
        public override string ToString() => $"Impedance {Name}";

    }
}
