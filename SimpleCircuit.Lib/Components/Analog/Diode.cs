using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;

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

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;

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
                _anchors = new(
                    new LabelAnchorPoint(new(0, -5), new(0, -1), Appearance),
                    new LabelAnchorPoint(new(0, 5), new(0, 1), Appearance));
            }

            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1), Appearance);
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
                            case 1: break;
                            case 2: break;
                        }

                        // Allow dashed/dotted lines
                        Appearance.LineStyle = Variants.Select(Dashed, Dotted) switch
                        {
                            0 => LineStyles.Dashed,
                            1 => LineStyles.Dotted,
                            _ => LineStyles.None
                        };
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance);
                switch (Variants.Select(_varactor, _zener, _tunnel, _schottky, _shockley, _tvs, _bidirectional))
                {
                    case 0: // Varactor
                        DrawVaractor(builder);
                        break;

                    case 1: // Zener
                        DrawZenerDiode(builder);
                        break;

                    case 2: // Tunnel
                        DrawTunnelDiode(builder);
                        break;

                    case 3: // Schottky
                        DrawSchottkyDiode(builder);
                        break;

                    case 4: // Shockley
                        DrawShockleyDiode(builder);
                        break;

                    case 5: // TVS
                        DrawTVSDiode(builder);
                        break;

                    case 6: // Bidirectional
                        DrawBidirectional(builder);
                        break;

                    default: // Just a regular diode
                        DrawJunctionDiode(builder);
                        break;
                }

                switch (Variants.Select(_photodiode, _led, _laser))
                {
                    case 0: DrawPhotodiode(builder); break;
                    case 1: DrawLed(builder); break;
                    case 2: DrawLaser(builder); break;
                }
                if (Variants.Contains(_stroke))
                {
                    var p1 = (FixedOrientedPin)Pins["anode"];
                    var p2 = (FixedOrientedPin)Pins["cathode"];
                    builder.Line(p1.Offset, p2.Offset, Appearance);
                }

                _anchors.Draw(builder, Labels);
            }

            private void DrawJunctionDiode(IGraphicsBuilder builder)
            {
                // The diode
                builder.Polygon([
                    new(-4, -4), 
                    new(4, 0),
                    new(-4, 4)
                ], Appearance);
                builder.Line(new(4, -4), new(4, 4), Appearance);
            }
            private void DrawZenerDiode(IGraphicsBuilder builder)
            {
                // The diode
                builder.Polygon([
                    new(-4, -4),
                    new(4, 0),
                    new(-4, 4)
                ], Appearance);
                switch (Variants.Select(_zenerSingle, _zenerSlanted))
                {
                    case 0:
                        builder.Polyline([
                            new(2, -4),
                            new(4, -4),
                            new(4, 4)
                        ], Appearance);
                        break;

                    case 1:
                        builder.Polyline([
                            new(2, -5),
                            new(4, -4),
                            new(4, 4),
                            new(6, 5)
                        ], Appearance);
                        if (_anchors[0].Location.Y > -6)
                            _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1), Appearance);
                        if (_anchors[1].Location.Y < 6)
                            _anchors[1] = new LabelAnchorPoint(new(0, 6), new(0, 1), Appearance);
                        break;

                    default:
                        builder.Polyline([
                            new(2, -4),
                            new(4, -4),
                            new(4, 4),
                            new(6, 4)
                        ], Appearance);
                        break;
                }
            }
            private void DrawTunnelDiode(IGraphicsBuilder builder)
            {
                // The diode
                builder.Polygon([
                    new(-4, -4),
                    new(4, 0),
                    new(-4, 4)
                ], Appearance);
                builder.Polyline([
                    new(2, -4),
                    new(4, -4),
                    new(4, 4),
                    new(2, 4)
                ], Appearance);
            }
            private void DrawSchottkyDiode(IGraphicsBuilder builder)
            {
                // The diode
                builder.Polygon([
                    new(-4, -4),
                    new(4, 0),
                    new(-4, 4)
                ], Appearance);
                builder.Polyline([
                    new(6, -3),
                    new(6, -4),
                    new(4, -4),
                    new(4, 4),
                    new(2, 4),
                    new(2, 3)
                ], Appearance);
            }
            private void DrawShockleyDiode(IGraphicsBuilder builder)
            {
                // The diode
                builder.Polygon([
                    new(-4, -4),
                    new(4, 0),
                    new(-4, 0)
                ], Appearance);
                builder.Line(new(-4, 0), new(-4, 4), Appearance);
                builder.Line(new(4, -4), new(4, 4), Appearance);
            }
            private void DrawVaractor(IGraphicsBuilder builder)
            {
                // The diode
                builder.Polygon([
                    new(-4, -4),
                    new(4, 0),
                    new(-4, 4)
                ], Appearance);
                builder.Line(new(4, -4), new(4, 4), Appearance);
                builder.Line(new(6, -4), new(6, 4), Appearance);
            }
            private void DrawPhotodiode(IGraphicsBuilder builder)
            {
                builder.Arrow(new(2, 7.5), new(1, 3.5), Appearance);
                builder.Arrow(new(-1, 9.5), new(-2, 5.5), Appearance);
                if (_anchors[1].Location.Y < 10.5)
                    _anchors[1] = new LabelAnchorPoint(new(0, 10.5), new(0, 1), Appearance);
            }
            private void DrawLed(IGraphicsBuilder builder)
            {
                builder.Arrow(new(1, 3.5), new(2, 7.5), Appearance);
                builder.Arrow(new(-2, 5.5), new(-1, 9.5), Appearance);
                if (_anchors[1].Location.Y < 10.5)
                    _anchors[1] = new LabelAnchorPoint(new(0, 10.5), new(0, 1), Appearance);
            }
            private void DrawLaser(IGraphicsBuilder builder)
            {
                builder.Line(new(0, -4), new(0, 4), Appearance);
                builder.Arrow(new(-2, 5), new(-2, 10), Appearance);
                builder.Arrow(new(2, 5), new(2, 10), Appearance);
                if (_anchors[1].Location.Y < 11)
                    _anchors[1] = new LabelAnchorPoint(new(0, 11), new(0, 1), Appearance);
            }
            private void DrawTVSDiode(IGraphicsBuilder builder)
            {
                // The diode
                builder.Polygon([
                    new(-4, -4),
                    new(4, 0),
                    new(-4, 4)
                ], Appearance);
                builder.Polygon([
                    new(4, 0),
                    new(12, -4),
                    new(12, 4)
                ], Appearance);
                builder.Polyline([
                    new(2, -5),
                    new(4, -4),
                    new(4, 4),
                    new(6, 5)
                ], Appearance);
                if (_anchors[1].Location.Y < 6)
                    _anchors[1] = new LabelAnchorPoint(new(0, 6), new(0, 1), Appearance);
            }
            private void DrawBidirectional(IGraphicsBuilder builder)
            {
                // The diode
                builder.Polygon([
                    new(-4, -4),
                    new(4, 0),
                    new(-4, 4)
                ], Appearance);
                builder.Polygon([
                    new(-4, -8),
                    new(4, -12),
                    new(4, -4)
                ], Appearance);
                builder.Line(new(-4, -4), new(-4, -12), Appearance);
                builder.Line(new(4, -4), new(4, 4), Appearance);
                if (_anchors[0].Location.Y > -13)
                    _anchors[0] = new LabelAnchorPoint(new(0, -13), new(0, -1), Appearance);
            }
        }
    }
}
