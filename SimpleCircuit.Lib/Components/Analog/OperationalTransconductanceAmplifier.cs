using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Analog;

/// <summary>
/// An Operational Transconductance Amplifier (OTA).
/// </summary>
[Drawable("TA", "A transconductance amplifier.", "Analog", "programmable", labelCount: 3)]
[Drawable("OTA", "A transconductance amplifier.", "Analog", "programmable", labelCount: 3)]
public class OperationalTransconductanceAmplifier : DrawableFactory
{
    private const string _differentialInput = "diffin";
    private const string _swapInput = "swapin";
    private const string _differentialOutput = "diffout";
    private const string _swapOutput = "swapout";
    private const string _programmable = "programmable";

    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
    {
        var result = new Instance(name);

        // We will just add a differential input as the default
        result.Variants.Add(_differentialInput);
        return result;
    }

    private class Instance : ScaledOrientedDrawable
    {
        private CustomLabelAnchorPoints _anchors;

        /// <inheritdoc />
        public override string Type => "ota";

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
                    Pins.Clear();
                    Vector2 la0 = new(-5, 9), la1 = new(5, 5);
                    Vector2 lb0 = new(5, -5), lb1 = new(-5, -9);

                    if (Variants.Contains(_differentialInput))
                    {
                        if (Variants.Contains(_swapInput))
                        {
                            AddPin(new FixedOrientedPin("negative", "The negative input.", this, new(-5, 4), new(-1, 0)), "n", "inn", "neg");
                            AddPin(new FixedOrientedPin("positive", "The positive input.", this, new(-5, -4), new(-1, 0)), "p", "inp", "pos");
                        }
                        else
                        {
                            AddPin(new FixedOrientedPin("negative", "The negative input.", this, new(-5, -4), new(-1, 0)), "n", "inn", "neg");
                            AddPin(new FixedOrientedPin("positive", "The positive input.", this, new(-5, 4), new(-1, 0)), "p", "inp", "pos");
                        }
                    }
                    else
                    {
                        AddPin(new FixedOrientedPin("input", "The input.", this, new(-5, 0), new(-1, 0)), "in", "inp", "input", "i");
                    }

                    // Add power pins
                    AddPin(new FixedOrientedPin("negativepower", "The negative power.", this, new(0, -7), new(0, -1)), "vneg", "vn");
                    AddPin(new FixedOrientedPin("positivepower", "The positive power.", this, new(0, 7), new(0, 1)), "vpos", "vp");

                    if (Variants.Contains(_differentialOutput))
                    {
                        if (Variants.Contains(_swapOutput))
                        {
                            AddPin(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(5, -4), new(1, 0)), "no", "outn");
                            AddPin(new FixedOrientedPin("positiveoutput", "The output.", this, new(5, 4), new(1, 0)), "o", "out", "outp", "output");
                            la1 = new(6, 8);
                            lb0 = new(8, -7);
                        }
                        else
                        {
                            AddPin(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(5, 4), new(1, 0)), "no", "outn");
                            AddPin(new FixedOrientedPin("positiveoutput", "The output.", this, new(5, -4), new(1, 0)), "o", "out", "outp", "output");
                            la1 = new(8, 7);
                            lb0 = new(6, -8);
                        }
                    }
                    else
                    {
                        AddPin(new FixedOrientedPin("output", "The output.", this, new(5, 0), new(1, 0)), "out", "outp", "output", "o");
                    }

                    if (Variants.Contains(_programmable))
                    {
                        if (la0.Y < 10)
                            la0 = new(-7, 10);
                        if (lb0.Y > -12)
                            lb0 = new(6, -12);
                    }

                    // Calculate the locations of the 2 outer anchor points
                    var expA = (la1 - la0).Perpendicular;
                    expA /= expA.Length;
                    var locA = Vector2.AtX(0.0, la0, la1) + expA * LabelMargin;
                    var expB = (lb1 - lb0).Perpendicular;
                    expB /= expB.Length;
                    var locB = Vector2.AtX(0.0, lb0, lb1) + expB * LabelMargin;
                    _anchors = new(
                        new LabelAnchorPoint(locA, expA),
                        new LabelAnchorPoint(new(0, 0), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.Center),
                        new LabelAnchorPoint(locB, expB));
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

            // The triangle
            builder.Polygon([
                new(-5, -9),
                new(5, -5),
                new(5, 5),
                new(-5, 9)
            ], style);

            // Pins and signs of the input
            if (Variants.Contains(_differentialInput))
            {
                builder.ExtendPins(Pins, style, 2, "inn", "inp");
                if (Variants.Contains(_swapInput))
                    builder.Signs(new(-2, -4), new(-2, 4), style);
                else
                    builder.Signs(new(-2, 4), new(-2, -4), style);
            }
            else
                builder.ExtendPin(Pins["n"], style);

            // Pins and signs of the output
            if (Variants.Contains(_differentialOutput))
            {
                builder.ExtendPins(Pins, style, 3, "outp", "outn");
                if (Variants.Contains(_swapOutput))
                    builder.Signs(new(6, 7), new(6, -7), style);
                else
                    builder.Signs(new(6, -7), new(6, 7), style);
            }
            else
                builder.ExtendPin(Pins["o"], style);

            // Programmable
            if (Variants.Contains(_programmable))
                builder.Arrow(new(-7, 10), new(6, -12), style);

            // Labels
            _anchors.Draw(builder, this, style);
        }
    }
}