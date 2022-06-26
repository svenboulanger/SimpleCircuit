using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;

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
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the diode.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "diode";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("anode", "The anode.", this, new(-4, 0), new(-1, 0)), "p", "a", "anode");
                Pins.Add(new FixedOrientedPin("cathode", "The cathode.", this, new(4, 0), new(1, 0)), "n", "c", "cathode");
                Variants.Changed += UpdatePins;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4), new(-4, 4)
                }, new("anode"));

                switch (Variants.Select(_varactor, _zener, _tunnel, _schottky))
                {
                    case 0: DrawVaractor(drawing); break;
                    case 1: DrawZenerDiode(drawing); break;
                    case 2: DrawTunnelDiode(drawing); break;
                    case 3: DrawSchottkyDiode(drawing); break;
                    default: DrawJunctionDiode(drawing); break;
                }

                switch (Variants.Select(_photodiode, _led))
                {
                    case 0: DrawPhotodiode(drawing); break;
                    case 1: DrawLed(drawing); break;
                }

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
            private void UpdatePins(object sender, EventArgs e)
                => SetPinOffset(1, new(Variants.Contains(_varactor) ? 6 : 4, 0));
        }
    }
}
