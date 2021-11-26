using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A diode.
    /// </summary>
    [SimpleKey("D", "A diode.", Category = "Analog")]
    public class Diode : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the diode.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Diode"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Diode(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("anode", "The anode.", this, new(-6, 0), new(-1, 0)), "p", "a", "anode");
            Pins.Add(new FixedOrientedPin("cathode", "The cathode.", this, new(6, 0), new(1, 0)), "n", "c", "cathode");

            DrawingVariants = Variant.All(
                Variant.If("photodiode").Do(DrawPhotodiode),
                Variant.If("led").Do(DrawLed),
                Variant.Do(DrawDiode));
        }

        /// <inheritdoc />
        private void DrawDiode(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new Vector2[]
            {
                new(-6, 0), new(-4, 0),
                new(4, 0), new(6, 0)
            }, new("wire"));

            // Diode
            drawing.Line(new(4, -4), new(4, 4), new("cathode"));
            drawing.Polygon(new[] {
                new Vector2(-4, -4),
                new Vector2(4, 0),
                new Vector2(-4, 4),
                new Vector2(-4, 4)
            }, new("anode"));

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
        }
        private void DrawPhotodiode(SvgDrawing drawing)
        {
            var opt = new PathOptions() { EndMarker = PathOptions.MarkerTypes.Arrow };
            drawing.Line(new(2, 7.5), new(1, 3.5), opt);
            drawing.Line(new(-1, 9.5), new(-2, 5.5), opt);
        }
        private void DrawLed(SvgDrawing drawing)
        {
            var opt = new PathOptions() { EndMarker = PathOptions.MarkerTypes.Arrow };
            drawing.Line(new(1, 3.5), new(2, 7.5), opt);
            drawing.Line(new(-2, 5.5), new(-1, 9.5), opt);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Diode {Name}";
    }
}
