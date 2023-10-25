using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A diode.
    /// </summary>
    [Drawable("D", "A diode.", "Analog", "varactor zener tunnel schottky schockley photodiode led laser tvs")]
    public class Diode : DrawableFactory
    {
        private const string _varactor = "varactor";
        private const string _zener = "zener";
        private const string _tunnel = "tunnel";
        private const string _schottky = "schottky";
        private const string _shockley = "shockley";
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
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -5), new(0, -1)),
                new LabelAnchorPoint(new(0, 5), new(0, 1)));

            /// <inheritdoc />
            public Labels Labels { get; } = new();

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

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1));
                switch (Variants.Select(_varactor, _zener, _tunnel, _schottky, _shockley, _tvs, _bidirectional))
                {
                    case 0: // Varactor
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(6, 0));
                        break;

                    case 1: // Zener
                    case 2: // Tunnel
                    case 3: // Schottky
                    case 4: // Shockley
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(4, 0));
                        break;

                    case 5: // TVS
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(12, 0));
                        break;

                    case 6: // Bidirectional
                        SetPinOffset(0, new(-4, -4));
                        SetPinOffset(1, new(4, -4));
                        break;

                    default:
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(4, 0));
                        break;
                }

                switch (Variants.Select(_photodiode, _led, _laser))
                {
                    case 0:
                    case 1:  break;
                    case 2:  break;
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                switch (Variants.Select(_varactor, _zener, _tunnel, _schottky, _shockley, _tvs, _bidirectional))
                {
                    case 0: // Varactor
                        DrawVaractor(drawing);
                        break;

                    case 1: // Zener
                        DrawZenerDiode(drawing);
                        break;

                    case 2: // Tunnel
                        DrawTunnelDiode(drawing);
                        break;

                    case 3: // Schottky
                        DrawSchottkyDiode(drawing);
                        break;

                    case 4: // Shockley
                        DrawShockleyDiode(drawing);
                        break;

                    case 5: // TVS
                        DrawTVSDiode(drawing);
                        break;

                    case 6: // Bidirectional
                        DrawBidirectional(drawing);
                        break;

                    default: // Just a regular diode
                        DrawJunctionDiode(drawing);
                        break;
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

                _anchors.Draw(drawing, this);
            }

            private void DrawJunctionDiode(SvgDrawing drawing)
            {
                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4)
                }, new("anode"));
                drawing.Line(new(4, -4), new(4, 4), new("cathode"));
            }
            private void DrawZenerDiode(SvgDrawing drawing)
            {
                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4)
                }, new("anode"));
                switch (Variants.Select(_zenerSingle, _zenerSlanted))
                {
                    case 0:
                        drawing.Polyline(new Vector2[] { new(2, -4), new(4, -4), new(4, 4) }, new("cathode"));
                        break;

                    case 1:
                        drawing.Polyline(new Vector2[] { new(2, -5), new(4, -4), new(4, 4), new(6, 5) }, new("cathode"));
                        if (_anchors[0].Location.Y > -6)
                            _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1));
                        if (_anchors[1].Location.Y < 6)
                            _anchors[1] = new LabelAnchorPoint(new(0, 6), new(0, 1));
                        break;

                    default:
                        drawing.Polyline(new Vector2[] { new(2, -4), new(4, -4), new(4, 4), new(6, 4) }, new("cathode"));
                        break;
                }
            }
            private void DrawTunnelDiode(SvgDrawing drawing)
            {
                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4)
                }, new("anode"));
                drawing.Polyline(new Vector2[] { new(2, -4), new(4, -4), new(4, 4), new(2, 4) }, new("cathode"));
            }
            private void DrawSchottkyDiode(SvgDrawing drawing)
            {
                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4)
                }, new("anode"));
                drawing.Polyline(new Vector2[] { new(6, -3), new(6, -4), new(4, -4), new(4, 4), new(2, 4), new(2, 3) }, new("cathode"));
            }
            private void DrawShockleyDiode(SvgDrawing drawing)
            {
                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 0)
                }, new("anode"));
                drawing.Line(new(-4, 0), new(-4, 4));
                drawing.Line(new(4, -4), new(4, 4));
            }
            private void DrawVaractor(SvgDrawing drawing)
            {
                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4)
                }, new("anode"));
                drawing.Line(new(4, -4), new(4, 4), new("cathode"));
                drawing.Line(new(6, -4), new(6, 4), new("cathode"));
            }
            private void DrawPhotodiode(SvgDrawing drawing)
            {
                drawing.Arrow(new(2, 7.5), new(1, 3.5));
                drawing.Arrow(new(-1, 9.5), new(-2, 5.5));
                if (_anchors[1].Location.Y < 10.5)
                    _anchors[1] = new LabelAnchorPoint(new(0, 10.5), new(0, 1));
            }
            private void DrawLed(SvgDrawing drawing)
            {
                drawing.Arrow(new(1, 3.5), new(2, 7.5));
                drawing.Arrow(new(-2, 5.5), new(-1, 9.5));
                if (_anchors[1].Location.Y < 10.5)
                    _anchors[1] = new LabelAnchorPoint(new(0, 10.5), new(0, 1));
            }
            private void DrawLaser(SvgDrawing drawing)
            {
                drawing.Line(new(0, -4), new(0, 4));
                drawing.Arrow(new(-2, 5), new(-2, 10));
                drawing.Arrow(new(2, 5), new(2, 10));
                if (_anchors[1].Location.Y < 11)
                    _anchors[1] = new LabelAnchorPoint(new(0, 11), new(0, 1));
            }
            private void DrawTVSDiode(SvgDrawing drawing)
            {
                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4)
                }, new("anode"));
                drawing.Polygon(new Vector2[] {
                    new(4, 0), new(12, -4), new(12, 4)
                }, new("anode2"));
                drawing.Polyline(new Vector2[] { new(2, -5), new(4, -4), new(4, 4), new(6, 5) }, new("cathode"));
                if (_anchors[1].Location.Y < 6)
                    _anchors[1] = new LabelAnchorPoint(new(0, 6), new(0, 1));
            }
            private void DrawBidirectional(SvgDrawing drawing)
            {
                // The diode
                drawing.Polygon(new Vector2[] {
                    new(-4, -4), new(4, 0), new(-4, 4)
                }, new("anode"));
                drawing.Polygon(new Vector2[]
                {
                    new(-4, -8), new(4, -12), new(4, -4)
                }, new("anode2"));
                drawing.Line(new(-4, -4), new(-4, -12));
                drawing.Line(new(4, -4), new(4, 4));
                if (_anchors[0].Location.Y > -13)
                    _anchors[0] = new LabelAnchorPoint(new(0, -13), new(0, -1));
            }
        }
    }
}
