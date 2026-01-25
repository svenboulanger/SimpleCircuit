using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Analog;

/// <summary>
/// A capacitor.
/// </summary>
[Drawable("C", "A capacitor.", "Analog", "electrolytic programmable sensor", labelCount: 2)]
public class Capacitor : DrawableFactory
{
    private const string _curved = "curved";
    private const string _signs = "signs";
    private const string _electrolytic = "electrolytic";
    private const string _programmable = "programmable";
    private const string _sensor = "sensor";
    private const string _asymmetric = "asymmetric";

    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors;

        /// <inheritdoc />
        public override string Type => "capacitor";

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
            AddPin(new FixedOrientedPin("pos", "The positive pin", this, new(-1.5, 0), new(-1, 0)), "p", "pos", "a");
            AddPin(new FixedOrientedPin("neg", "the negative pin", this, new(1.5, 0), new(1, 0)), "n", "neg", "b");
            _anchors = new(2);
        }

        /// <inheritdoc />
        public override PresenceResult Prepare(IPrepareContext context)
        {
            var result = base.Prepare(context);
            if (result == PresenceResult.GiveUp)
                return result;

            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    if (Variants.Contains(_electrolytic))
                    {
                        SetPinOffset(Pins["a"], new(-2.25, 0));
                        SetPinOffset(Pins["b"], new(2.25, 0));
                    }
                    else
                    {
                        SetPinOffset(Pins["a"], new(-1.5, 0));
                        SetPinOffset(Pins["b"], new(1.5, 0));
                    }
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            builder.ExtendPins(Pins, style, 3.5);

            // Because the component is symmetrical, we can keep it upright in an even better way
            bool applyTransform = KeepUpright && !Variants.Contains(_asymmetric);
            if (applyTransform)
            {
                builder.BeginTransform(new(Vector2.Zero, Drawing.Matrix2.Scale(
                    builder.CurrentTransform.Matrix.A11 < 0.0 ? -1.0 : 1.0,
                    builder.CurrentTransform.Matrix.A22 < 0.0 ? -1.0 : 1.0)));
            }

            switch (Variants.Select(_curved, _electrolytic))
            {
                case 0:
                    // Plates
                    builder.Line(new(-1.5, -4), new(-1.5, 4), style.AsLineThickness(1.0));
                    builder.Path(b => b.MoveTo(new(3, -4)).CurveTo(new(1.5, -2), new(1.5, -0.5), new(1.5, 0)).SmoothTo(new(1.5, 2), new(3, 4)), style.AsStroke());
                    if (Variants.Contains(_signs))
                        builder.Signs(new Vector2(-4, 3), new Vector2(5, 3), style, vertical: true);
                    _anchors[0] = new LabelAnchorPoint(new(0, -4.5 - LabelMargin), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 4.5 + LabelMargin), new(0, 1));
                    break;

                case 1:
                    // Assymetric plates
                    builder.Rectangle(-2.25, -4, 1.5, 8, style);
                    builder.Rectangle(0.75, -4, 1.5, 8, style.AsFilledMarker());
                    if (Variants.Contains(_signs))
                        builder.Signs(new(-5, 3), new(5, 3), style, vertical: true);
                    _anchors[0] = new LabelAnchorPoint(new(0, -4 - style.LineThickness * 0.5 - LabelMargin), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 4 + style.LineThickness * 0.5 + LabelMargin), new(0, 1));
                    break;

                default:
                    // Plates
                    var plateStyle = style.AsLineThickness(1.0);
                    builder.Line(new(-1.5, -4), new(-1.5, 4), plateStyle);
                    builder.Line(new(1.5, -4), new(1.5, 4), plateStyle);
                    if (Variants.Contains(_signs))
                        builder.Signs(new(-4, 3), new(4, 3), style, vertical: true);
                    _anchors[0] = new LabelAnchorPoint(new(0, -4.5 - LabelMargin), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 4.5 + LabelMargin), new(0, 1));
                    break;
            }

            switch (Variants.Select(_programmable, _sensor))
            {
                case 0:
                    builder.Arrow(new(-4, 4), new(6, -5), style);
                    _anchors[0] = new LabelAnchorPoint(new(0, -5 - style.LineThickness * 0.5 - LabelMargin), new(0, -1));
                    break;

                case 1:
                    builder.Polyline([
                        new(-6, 6),
                        new(-4, 6),
                        new(4, -6)
                    ], style);
                    double m = style.LineThickness * 0.5 + LabelMargin;
                    _anchors[0] = new LabelAnchorPoint(new(0, -6 - m), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 6 + m), new(0, 1));
                    break;
            }
            _anchors.Draw(builder, this, style);

            if (applyTransform)
                builder.EndTransform();
        }
    }
}