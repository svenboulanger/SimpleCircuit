using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A capacitor.
    /// </summary>
    [SimpleKey("C", "A capacitor.", Category = "Analog")]
    public class Capacitor : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the capacitor.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Capacitor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Capacitor(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("pos", "The positive pin", this, new(-5, 0), new(-1, 0)), "p", "pos", "a");
            Pins.Add(new FixedOrientedPin("neg", "the negative pin", this, new(5, 0), new(1, 0)), "n", "neg", "b");

            if (options?.PolarCapacitors ?? false)
                AddVariant("polar");
            DrawingVariants = Variant.All(
                Variant.If("polar").DoElse(DrawPolar, DrawApolar),
                Variant.If("programmable").Do(DrawProgrammable));
        }

        private void DrawPolar(SvgDrawing drawing)
        {
            drawing.Segments(new Vector2[] {
                new(-5, 0), new(-1.5, 0),
                new(1, 0), new(5, 0)
            }, new("wire"));

            drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos"));
            drawing.OpenBezier(new Vector2[]
            {
                    new(2.5, -4),
                    new(1, -2), new(1, -0.5), new(1, 0),
                    new(1, 0.5), new(1, 2), new(2.5, 4)
            }, new("neg"));
            drawing.Segments(new Vector2[]
            {
                    new(-4, 2), new(-4, 4),
                    new(-5, 3), new(-3, 3)
            }, new("plus"));
            drawing.Line(new(5, 2), new(5, 4), new("minus"));
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
        }
        private void DrawApolar(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new Vector2[]
            {
                new(-5, 0), new(-1.5, 0),
                new(1.5, 0), new(5, 0)
            }, new("wire"));

            drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos"));
            drawing.Line(new(1.5, -4), new(1.5, 4), new("neg"));

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
        }
        private void DrawProgrammable(SvgDrawing drawing)
        {
            var options = new PathOptions() { EndMarker = PathOptions.MarkerTypes.Arrow };
            drawing.Polyline(new Vector2[] { new(-4, 4), new(6, -5) }, options);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Capacitor {Name}";
    }
}
