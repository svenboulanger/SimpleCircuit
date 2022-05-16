using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A diode.
    /// </summary>
    [Drawable("D", "A diode.", "Analog")]
    public class Diode : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the diode.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "diode";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("anode", "The anode.", this, new(-6, 0), new(-1, 0)), "p", "a", "anode");
                Pins.Add(new FixedOrientedPin("cathode", "The cathode.", this, new(6, 0), new(1, 0)), "n", "c", "cathode");

                DrawingVariants = Variant.All(
                    Variant.If("photodiode").Then(DrawPhotodiode),
                    Variant.If("led").Then(DrawLed),
                    Variant.Do(DrawDiode));
            }

            /// <inheritdoc />
            private void DrawDiode(SvgDrawing drawing)
            {
                drawing.Path(b => b.MoveTo(-6, 0).LineTo(-4, 0).MoveTo(4, 0).LineTo(6, 0), new("wire"));
                drawing.Line(new(4, -4), new(4, 4), new("cathode"));
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4), new(-4, 4)
                }, new("anode"));
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -6), new(0, -1));
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
        }
    }
}
