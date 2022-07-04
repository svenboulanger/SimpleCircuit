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
        private const string _laser = "laser";
        private const string _zenerSingle = "single";
        private const string _zenerSlanted = "slanted";
        private const string _tvs = "tvs";
        private const string _bidirectional = "bidirectional";
        private const string _stroke = "stroke";

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
            }

            public override void Reset()
            {
                base.Reset();
                switch (Variants.Select(_varactor, _zener, _tunnel, _schottky, _tvs, _bidirectional))
                {
                    case 0:
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(6, 0));
                        break;

                    case 1:
                    case 2:
                    case 3:
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(4, 0));
                        break;

                    case 4:
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(12, 0));
                        break;

                    case 5:
                        SetPinOffset(0, new(-4, -4));
                        SetPinOffset(1, new(4, -4));
                        break;

                    default:
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(4, 0));
                        break;
                }
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4)
                }, new("anode"));

                switch (Variants.Select(_varactor, _zener, _tunnel, _schottky, _tvs, _bidirectional))
                {
                    case 0: DrawVaractor(drawing); break;
                    case 1: DrawZenerDiode(drawing); break;
                    case 2: DrawTunnelDiode(drawing); break;
                    case 3: DrawSchottkyDiode(drawing); break;
                    case 4: DrawTVSDiode(drawing); break;
                    case 5: DrawBidirectional(drawing); break;
                    default: DrawJunctionDiode(drawing); break;
                }

                switch (Variants.Select(_photodiode, _led, _laser))
                {
                    case 0: DrawPhotodiode(drawing); break;
                    case 1: DrawLed(drawing); break;
                    case 2: DrawLaser(drawing); break;
                }
                if (Variants.Contains(_stroke))
                {
                    var p1 = (FixedOrientedPin)Pins["anode"];
                    var p2 = (FixedOrientedPin)Pins["cathode"];
                    drawing.Line(p1.Offset, p2.Offset, new("stroke"));
                }

                // Label
                drawing.Text(Label, new(0, -6), new(0, -1));
            }

            private void DrawJunctionDiode(SvgDrawing drawing) => drawing.Line(new(4, -4), new(4, 4), new("cathode"));
            private void DrawZenerDiode(SvgDrawing drawing)
            {
                switch (Variants.Select(_zenerSingle, _zenerSlanted))
                {
                    case 0:
                        drawing.Polyline(new Vector2[] { new(2, -4), new(4, -4), new(4, 4) }, new("cathode"));
                        break;

                    case 1:
                        drawing.Polyline(new Vector2[] { new(2, -5), new(4, -4), new(4, 4), new(6, 5) }, new("cathode"));
                        break;

                    default:
                        drawing.Polyline(new Vector2[] { new(2, -4), new(4, -4), new(4, 4), new(6, 4) }, new("cathode"));
                        break;
                }

            }
            private void DrawTunnelDiode(SvgDrawing drawing) => drawing.Polyline(new Vector2[] { new(2, -4), new(4, -4), new(4, 4), new(2, 4) }, new("cathode"));
            private void DrawSchottkyDiode(SvgDrawing drawing) => drawing.Polyline(new Vector2[] { new(6, -3), new(6, -4), new(4, -4), new(4, 4), new(2, 4), new(2, 3) }, new("cathode"));
            private void DrawVaractor(SvgDrawing drawing)
            {
                drawing.Line(new(4, -4), new(4, 4), new("cathode"));
                drawing.Line(new(6, -4), new(6, 4), new("cathode"));
            }
            private void DrawPhotodiode(SvgDrawing drawing)
            {
                drawing.Arrow(new(2, 7.5), new(1, 3.5));
                drawing.Arrow(new(-1, 9.5), new(-2, 5.5));
            }
            private void DrawLed(SvgDrawing drawing)
            {
                drawing.Arrow(new(1, 3.5), new(2, 7.5));
                drawing.Arrow(new(-2, 5.5), new(-1, 9.5));
            }
            private void DrawLaser(SvgDrawing drawing)
            {
                drawing.Line(new(0, -4), new(0, 4));
                drawing.Arrow(new(-2, 5), new(-2, 10));
                drawing.Arrow(new(2, 5), new(2, 10));
            }
            private void DrawTVSDiode(SvgDrawing drawing)
            {
                drawing.Polyline(new Vector2[] { new(2, -5), new(4, -4), new(4, 4), new(6, 5) }, new("cathode"));
                drawing.Polygon(new Vector2[] {
                    new(4, 0), new(12, -4), new(12, 4)
                }, new("anode2"));
            }
            private void DrawBidirectional(SvgDrawing drawing)
            {
                drawing.Line(new(-4, -4), new(-4, -12));
                drawing.Line(new(4, -4), new(4, 4));
                drawing.Polygon(new Vector2[]
                {
                    new(-4, -8), new(4, -12), new(4, -4)
                }, new("anode2"));
            }
        }
    }
}
