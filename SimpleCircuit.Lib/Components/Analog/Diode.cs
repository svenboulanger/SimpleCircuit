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
        private const string _varactor = "varactor";
        private const string _zener = "zener";
        private const string _tunnel = "tunnel";
        private const string _schottky = "schottky";
        private const string _photodiode = "photodiode";
        private const string _led = "led";

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
                Pins.Add(new FixedOrientedPin("anode", "The anode.", this, new(-4, 0), new(-1, 0)), "p", "a", "anode");
                Pins.Add(new FixedOrientedPin("cathode", "The cathode.", this, new(4, 0), new(1, 0)), "n", "c", "cathode");

                PinUpdate = Variant.Map(_varactor, UpdatePins);
                DrawingVariants = Variant.All(
                    Variant.If(_photodiode).Then(DrawPhotodiode),
                    Variant.If(_led).Then(DrawLed),
                    Variant.FirstOf(
                        Variant.If(_varactor).Then(DrawVaractor),
                        Variant.If(_zener).Then(DrawZenerDiode),
                        Variant.If(_tunnel).Then(DrawTunnelDiode),
                        Variant.If(_schottky).Then(DrawSchottkyDiode),
                        Variant.Do(DrawJunctionDiode)),
                    Variant.Do(DrawDiode));
            }

            /// <inheritdoc />
            private void DrawDiode(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.ExtendPin(Pins[0]);
                if (Pins[1].Connections == 0)
                    drawing.ExtendPin(Pins[1]);

                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4), new(-4, 4)
                }, new("anode"));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -6), new(0, -1));
            }
            private void DrawJunctionDiode(SvgDrawing drawing) => drawing.Line(new(4, -4), new(4, 4), new("cathode"));
            private void DrawZenerDiode(SvgDrawing drawing) => drawing.Polyline(new Vector2[] { new(6, -4), new(4, -4), new(4, 4), new(2, 4) }, new("cathode"));
            private void DrawTunnelDiode(SvgDrawing drawing) => drawing.Polyline(new Vector2[] { new(2, -4), new(4, -4), new(4, 4), new(2, 4) }, new("cathode"));
            private void DrawSchottkyDiode(SvgDrawing drawing) => drawing.Polyline(new Vector2[] { new(6, -3), new(6, -4), new(4, -4), new(4, 4), new(2, 4), new(2, 3) }, new("cathode"));
            private void DrawVaractor(SvgDrawing drawing)
            {
                drawing.Line(new(4, -4), new(4, 4), new("cathode"));
                drawing.Line(new(6, -4), new(6, 4), new("cathode"));
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
            private void UpdatePins(bool isVaractor)
                => SetPinOffset(1, new(isVaractor ? 6 : 4, 0));
        }
    }
}
