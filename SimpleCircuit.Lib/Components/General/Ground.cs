using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components;

/// <summary>
/// A ground terminal.
/// </summary>
[Drawable("GND", "A common ground symbol.", "General", "earth chassis vss vee", labelCount: 2)]
[Drawable("SGND", "A signal ground symbol.", "General", "earth chassis vss vee", labelCount: 2)]
public class Ground : DrawableFactory
{
    private const string _earth = "earth";
    private const string _chassis = "chassis";
    private const string _signal = "signal";
    private const string _noiseless = "noiseless";
    private const string _protective = "protective";

    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
    {
        var device = new Instance(name);
        if (key == "SGND")
            device.Variants.Add("signal");
        return device;
    }

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors;
        /// <inheritdoc />
        public override string Type => "ground";

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
            AddPin(new FixedOrientedPin("p", "The one and only pin.", this, new(0, 0), new(0, -1)), "a", "p");
            _anchors = new(2);
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            double m = style.LineThickness * 0.5 + LabelMargin;
            _anchors[0] = new LabelAnchorPoint(new(-5 - m, 0), new(-1, 0));
            _anchors[1] = new LabelAnchorPoint(new(5 + m, 0), new(1, 0));
            switch (Variants.Select(_earth, _chassis, _signal))
            {
                case 0:
                case 1: DrawEarth(builder, style); break;
                case 2: DrawSignalGround(builder, style); break;
                default: DrawGround(builder, style); break;
            }

            _anchors.Draw(builder, this, style);
        }
        private void DrawGround(IGraphicsBuilder drawing, IStyle style)
        {
            switch (Variants.Select(_noiseless, _protective))
            {
                case 0:
                    drawing.Path(b => b.MoveTo(new(-8, 4)).ArcTo(8, 8, 0, true, true, new(8, 4)), style);
                    drawing.ExtendPins(Pins, style, 6);

                    double m = style.LineThickness * 0.5 + LabelMargin;
                    if (_anchors[0].Location.X > -8 - m)
                        _anchors[0] = new LabelAnchorPoint(new(-8 - m, 0), new(-1, 0));
                    if (_anchors[1].Location.X < 8 + m)
                        _anchors[1] = new LabelAnchorPoint(new(8 + m, 0), new(1, 0));

                    break;

                case 1:
                    drawing.Circle(new(0, -1), 6.5, style);
                    drawing.ExtendPins(Pins, style, 7.5);

                    m = style.LineThickness * 0.5 + LabelMargin;
                    if (_anchors[0].Location.X > -6.5 - m) 
                        _anchors[0] = new LabelAnchorPoint(new(-6.5 - m, 0), new(-1, 0));
                    if (_anchors[1].Location.X < 6.5 + m)
                        _anchors[1] = new LabelAnchorPoint(new(6.5 + m, 0), new(1, 0));

                    break;

                default:
                    drawing.ExtendPins(Pins, style, 3);
                    break;
            }

            drawing.Path(b => b
                .MoveTo(new(-5, 0))
                .LineTo(new(5, 0))
                .MoveTo(new(-3, 2))
                .LineTo(new(3, 2))
                .MoveTo(new(-1, 4))
                .LineTo(new(1, 4)), style.AsStroke());
        }
        private void DrawEarth(IGraphicsBuilder drawing, IStyle style)
        {
            drawing.ExtendPins(Pins, style, 3);

            // Ground segments
            drawing.Path(b => b
                .MoveTo(new(-5, 0))
                .LineTo(new(5, 0))
                .MoveTo(new(-5, 0))
                .Line(new(-2, 4))
                .MoveTo(new(0, 0))
                .Line(new(-2, 4))
                .MoveTo(new(5, 0))
                .Line(new(-2, 4)), style);

            double m = style.LineThickness * 0.5 + LabelMargin;
            if (_anchors[0].Location.X > -7 - m)
                _anchors[0] = new LabelAnchorPoint(new(-7 - m, 0), new(-1, 0));

        }
        private void DrawSignalGround(IGraphicsBuilder drawing, IStyle style)
        {
            drawing.Polygon([
                new(-5, 0),
                new(5, 0),
                new(0, 4)
            ], style);

            drawing.ExtendPins(Pins, style, 3);
        }
    }
}
