using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Analog;

/// <summary>
/// A factory for amplifiers.
/// </summary>
[Drawable("A", "A generic amplifier.", "Analog", "schmitt trigger comparator programmable", labelCount: 3)]
[Drawable("OA", "An operational amplifier.", "Analog", "schmitt trigger comparator programmable", labelCount: 3)]
public class Amplifier : DrawableFactory
{
    private const string _differentialInput = "diffin";
    private const string _swapInput = "swapin";
    private const string _differentialOutput = "diffout";
    private const string _swapOutput = "swapout";
    private const string _schmitt = "schmitt";
    private const string _comparator = "comparator";
    private const string _programmable = "programmable";

    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
    {
        switch (key)
        {
            case "OA":
                var result = new Instance(name);
                result.Variants.Add(_differentialInput);
                return result;

            default:
                return new Instance(name);
        }
    }

    /// <summary>
    /// Creates a new <see cref="Instance"/>.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="Instance"/>.
    /// </remarks>
    /// <param name="name">The name.</param>
    private class Instance(string name) : ScaledOrientedDrawable(name)
    {
        private readonly static Vector2
            _supplyPos = new(-2, -5),
            _supplyNeg = new(-2, 5),
            _inputPos = new(-8, -4),
            _inputNeg = new(-8, 4),
            _inputCommon = new(-8, 0),
            _outputPos = new(0, 4),
            _outputNeg = new(0, -4),
            _outputCommon = new(8, 0);
        private CustomLabelAnchorPoints _anchors = null;

        /// <inheritdoc />
        public override string Type => "amplifier";

        [Description("The margin for labels.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <inheritdoc />
        public override PresenceResult Prepare(IPrepareContext context)
        {
            var result = base.Prepare(context);
            if (result == PresenceResult.GiveUp)
                return result;

            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    Pins.Clear();
                    Vector2 la0 = new(-8, 8), la1 = new(8, 0);
                    Vector2 lb0 = new(8, 0), lb1 = new(-8, -8);

                    // Add input pins
                    if (Variants.Contains(_differentialInput))
                    {
                        if (Variants.Contains(_swapInput))
                        {
                            AddPin(new FixedOrientedPin("positiveinput", "The (positive) input.", this, _inputNeg, new(-1, 0)), "i", "in", "inp", "pi", "p");
                            AddPin(new FixedOrientedPin("negativeinput", "The negative input.", this, _inputPos, new(-1, 0)), "inn", "ni", "n");
                        }
                        else
                        {
                            AddPin(new FixedOrientedPin("positiveinput", "The (positive) input.", this, _inputPos, new(-1, 0)), "i", "in", "inp", "pi", "p");
                            AddPin(new FixedOrientedPin("negativeinput", "The negative input.", this, _inputNeg, new(-1, 0)), "inn", "ni", "n");
                        }
                    }
                    else
                        AddPin(new FixedOrientedPin("input", "The input.", this, _inputCommon, new(-1, 0)), "i", "in", "inp", "pi", "p");

                    // Add power supply pins
                    AddPin(new FixedOrientedPin("positivepower", "The positive power supply.", this, _supplyPos, new(0, -1)), "vpos", "vp");
                    AddPin(new FixedOrientedPin("negativepower", "The negative power supply.", this, _supplyNeg, new(0, 1)), "vneg", "vn");

                    // Add output pins
                    if (Variants.Contains(_differentialOutput))
                    {
                        if (Variants.Contains(_swapOutput))
                        {
                            AddPin(new FixedOrientedPin("negativeoutput", "The negative output.", this, _outputPos, new(1, 0)), "outn", "no");
                            AddPin(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, _outputNeg, new(1, 0)), "o", "out", "outp", "po");
                            la1 = new(7, 7);
                            lb0 = new(6, -8);
                        }
                        else
                        {
                            AddPin(new FixedOrientedPin("negativeoutput", "The negative output.", this, _outputNeg, new(1, 0)), "outn", "no");
                            AddPin(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, _outputPos, new(1, 0)), "o", "out", "outp", "po");
                            la1 = new(6, 8);
                            lb0 = new(7, -7);
                        }
                    }
                    else
                        AddPin(new FixedOrientedPin("output", "The output.", this, _outputCommon, new(1, 0)), "o", "out", "outp", "po");

                    if (Variants.Contains(_programmable))
                    {
                        la0 = new(-7, 10);
                        lb0 = new(4, -8.5);
                    }

                    // Calculate the locations of the 2 outer anchor points
                    var style = context.Style.ModifyDashedDotted(this);
                    double m = style.LineThickness * 0.5 + LabelMargin;
                    var expA = (la1 - la0).Perpendicular;
                    expA /= expA.Length;
                    var locA = Vector2.AtX(2.0, la0, la1) + expA * m;
                    var expB = (lb1 - lb0).Perpendicular;
                    expB /= expB.Length;
                    var locB = Vector2.AtX(2.0, lb0, lb1) + expB * m;
                    switch (Variants.Select(_schmitt, _comparator))
                    {
                        case 0:
                        case 1:
                            // There's already some diagram in the center, don't allocate the label anchor inside the amplifier
                            _anchors = new(
                                new(locA, expA),
                                new(locB, expB));
                            break;

                        default:
                            _anchors = new(
                                new(locA, expA),
                                new(new(-2.5, 0), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.Center),
                                new(locB, expB));
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

            // The main triangle
            builder.Polygon(
            [
                new(-8, -8),
                new(8, 0),
                new(-8, 8)
            ], style);

            // Differential input?
            var markerStyle = StrokeMarkerStyleModifier.Default.Apply(style);
            if (Variants.Contains(_differentialInput))
            {
                builder.ExtendPins(Pins, style, 2, "inp", "inn");
                if (Variants.Contains(_swapInput))
                    builder.Signs(new(-5.5, 4), new(-5.5, -4), style);
                else
                    builder.Signs(new(-5.5, -4), new(-5.5, 4), style);
            }
            else
                builder.ExtendPin(Pins["in"], style);

            // Differential output?
            if (Variants.Contains(_differentialOutput))
            {
                builder.ExtendPins(Pins, style, 5, "outp", "outn");
                if (Variants.Contains(_swapOutput))
                    builder.Signs(new(6, -7), new(6, 7), style);
                else
                    builder.Signs(new(6, 7), new(6, -7), style);
            }
            else
                builder.ExtendPin(Pins["out"], style);

            // Programmable arrow
            if (Variants.Contains(_programmable))
                builder.Arrow(new(-7, 10), new(4, -8.5), style);

            // Comparator or schmitt trigger
            switch (Variants.Select(_comparator, _schmitt))
            {
                case 0: // Comparator
                    builder.Path(b => b.MoveTo(new(-5, 2))
                    .LineTo(new(-3, 2))
                    .LineTo(new(-3, -2))
                    .LineTo(new(-1, -2)), style.AsStrokeMarker(Style.DefaultLineThickness));
                    break;

                case 1: // Schmitt trigger
                    builder.Path(b =>
                    {
                        b.MoveTo(new(-6, 2))
                            .LineTo(new(-4, 2))
                            .LineTo(new(-4, -2))
                            .LineTo(new(-2, -2));
                        b.MoveTo(new(-4, 2))
                            .LineTo(new(-2, 2))
                            .LineTo(new(-2, -2))
                            .LineTo(new(0, -2));
                    }, style.AsStrokeMarker(Style.DefaultLineThickness));
                    break;
            }

            // Draw the labels
            _anchors.Draw(builder, this, style);
        }
    }
}
