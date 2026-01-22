using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Analog;

/// <summary>
/// A diode.
/// </summary>
[Drawable("D", "A diode.", "Analog", "varactor zener tunnel schottky schockley photodiode led laser tvs", labelCount: 2)]
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
        private readonly CustomLabelAnchorPoints _anchors = new(2);

        /// <inheritdoc />
        public override string Type => "diode";

        [Description("The margin for labels.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

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

        public override PresenceResult Prepare(IPrepareContext context)
        {
            var result = base.Prepare(context);
            if (result == PresenceResult.GiveUp)
                return result;

            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    var style = context.Style.ModifyDashedDotted(this);
                    double m = style.LineThickness * 0.5 + LabelMargin;
                    _anchors[0] = new LabelAnchorPoint(new(0, -4 - m), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 4 + m), new(0, 1));
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
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

            builder.ExtendPins(Pins, style);
            switch (Variants.Select(_varactor, _zener, _tunnel, _schottky, _shockley, _tvs, _bidirectional))
            {
                case 0: // Varactor
                    DrawVaractor(builder, style);
                    break;

                case 1: // Zener
                    DrawZenerDiode(builder, style);
                    break;

                case 2: // Tunnel
                    DrawTunnelDiode(builder, style);
                    break;

                case 3: // Schottky
                    DrawSchottkyDiode(builder, style);
                    break;

                case 4: // Shockley
                    DrawShockleyDiode(builder, style);
                    break;

                case 5: // TVS
                    DrawTVSDiode(builder, style);
                    break;

                case 6: // Bidirectional
                    DrawBidirectional(builder, style);
                    break;

                default: // Just a regular diode
                    DrawJunctionDiode(builder, style);
                    break;
            }

            switch (Variants.Select(_photodiode, _led, _laser))
            {
                case 0: DrawPhotodiode(builder, style); break;
                case 1: DrawLed(builder, style); break;
                case 2: DrawLaser(builder, style); break;
            }
            if (Variants.Contains(_stroke))
            {
                var p1 = (FixedOrientedPin)Pins["anode"];
                var p2 = (FixedOrientedPin)Pins["cathode"];
                builder.Line(p1.Offset, p2.Offset, style);
            }

            _anchors.Draw(builder, this, style);
        }

        private void DrawJunctionDiode(IGraphicsBuilder builder, IStyle style)
        {
            // The diode
            builder.Polygon([
                new(-4, -4), 
                new(4, 0),
                new(-4, 4)
            ], style);
            builder.Line(new(4, -4), new(4, 4), style);
        }
        private void DrawZenerDiode(IGraphicsBuilder builder, IStyle style)
        {
            // The diode
            builder.Polygon([
                new(-4, -4),
                new(4, 0),
                new(-4, 4)
            ], style);
            switch (Variants.Select(_zenerSingle, _zenerSlanted))
            {
                case 0:
                    builder.Polyline([
                        new(2, -4),
                        new(4, -4),
                        new(4, 4)
                    ], style);
                    break;

                case 1:
                    builder.Polyline([
                        new(2, -5),
                        new(4, -4),
                        new(4, 4),
                        new(6, 5)
                    ], style);
                    double m = style.LineThickness * 0.5 + LabelMargin;
                    if (_anchors[0].Location.Y > -5 - m)
                        _anchors[0] = new LabelAnchorPoint(new(0, -5 - m), new(0, -1));
                    if (_anchors[1].Location.Y < 5 + m)
                        _anchors[1] = new LabelAnchorPoint(new(0, 5 + m), new(0, 1));
                    break;

                default:
                    builder.Polyline([
                        new(2, -4),
                        new(4, -4),
                        new(4, 4),
                        new(6, 4)
                    ], style);
                    break;
            }
        }
        private void DrawTunnelDiode(IGraphicsBuilder builder, IStyle style)
        {
            // The diode
            builder.Polygon([
                new(-4, -4),
                new(4, 0),
                new(-4, 4)
            ], style);
            builder.Polyline([
                new(2, -4),
                new(4, -4),
                new(4, 4),
                new(2, 4)
            ], style);
        }
        private void DrawSchottkyDiode(IGraphicsBuilder builder, IStyle style)
        {
            // The diode
            builder.Polygon([
                new(-4, -4),
                new(4, 0),
                new(-4, 4)
            ], style);
            builder.Polyline([
                new(6, -3),
                new(6, -4),
                new(4, -4),
                new(4, 4),
                new(2, 4),
                new(2, 3)
            ], style);
        }
        private void DrawShockleyDiode(IGraphicsBuilder builder, IStyle style)
        {
            // The diode
            builder.Polygon([
                new(-4, -4),
                new(4, 0),
                new(-4, 0)
            ], style);
            builder.Line(new(-4, 0), new(-4, 4), style);
            builder.Line(new(4, -4), new(4, 4), style);
        }
        private void DrawVaractor(IGraphicsBuilder builder, IStyle style)
        {
            // The diode
            builder.Polygon([
                new(-4, -4),
                new(4, 0),
                new(-4, 4)
            ], style);
            builder.Line(new(4, -4), new(4, 4), style);
            builder.Line(new(6, -4), new(6, 4), style);
        }
        private void DrawPhotodiode(IGraphicsBuilder builder, IStyle style)
        {
            builder.Arrow(new(2, 7.5), new(1, 3.5), style);
            builder.Arrow(new(-1, 9.5), new(-2, 5.5), style);
            double m = style.LineThickness * 0.5 + LabelMargin;
            if (_anchors[1].Location.Y < 9.5 + m)
                _anchors[1] = new LabelAnchorPoint(new(0, 9.5 + m), new(0, 1));
        }
        private void DrawLed(IGraphicsBuilder builder, IStyle style)
        {
            builder.Arrow(new(1, 3.5), new(2, 7.5), style);
            builder.Arrow(new(-2, 5.5), new(-1, 9.5), style);
            double m = style.LineThickness * 0.5 + LabelMargin;
            if (_anchors[1].Location.Y < 9.5 + m)
                _anchors[1] = new LabelAnchorPoint(new(0, 9.5 + m), new(0, 1));
        }
        private void DrawLaser(IGraphicsBuilder builder, IStyle style)
        {
            builder.Line(new(0, -4), new(0, 4), style);
            builder.Arrow(new(-2, 5), new(-2, 10), style);
            builder.Arrow(new(2, 5), new(2, 10), style);

            double m = style.LineThickness * 0.5 + LabelMargin;
            if (_anchors[1].Location.Y < 10 + m)
                _anchors[1] = new LabelAnchorPoint(new(0, 10 + m), new(0, 1));
        }
        private void DrawTVSDiode(IGraphicsBuilder builder, IStyle style)
        {
            // The diode
            builder.Polygon([
                new(-4, -4),
                new(4, 0),
                new(-4, 4)
            ], style);
            builder.Polygon([
                new(4, 0),
                new(12, -4),
                new(12, 4)
            ], style);
            builder.Polyline([
                new(2, -5),
                new(4, -4),
                new(4, 4),
                new(6, 5)
            ], style);

            double m = style.LineThickness * 0.5 + LabelMargin;
            if (_anchors[1].Location.Y < 5 + m)
                _anchors[1] = new LabelAnchorPoint(new(0, 5 + m), new(0, 1));
        }
        private void DrawBidirectional(IGraphicsBuilder builder, IStyle style)
        {
            // The diode
            builder.Polygon([
                new(-4, -4),
                new(4, 0),
                new(-4, 4)
            ], style);
            builder.Polygon([
                new(-4, -8),
                new(4, -12),
                new(4, -4)
            ], style);
            builder.Line(new(-4, -4), new(-4, -12), style);
            builder.Line(new(4, -4), new(4, 4), style);

            double m = style.LineThickness * 0.5 + LabelMargin;
            if (_anchors[0].Location.Y > -12 - m)
                _anchors[0] = new LabelAnchorPoint(new(0, -12 - m), new(0, -1));
        }
    }
}
