using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA).
    /// </summary>
    [Drawable(new[] { "OTA", "TA" }, "A transconductance amplifier.", new[] { "Analog" })]
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

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private readonly static CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(2, 8), new(1, 1)),
                new LabelAnchorPoint(new(), new()),
                new LabelAnchorPoint(new(2, -8), new(1, -1)));

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "ota";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative input.", this, new(-5, -4), new(-1, 0)), "n", "inn", "neg");
                Pins.Add(new FixedOrientedPin("positive", "The positive input.", this, new(-5, 4), new(-1, 0)), "p", "inp", "pos");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power.", this, new(0, -7), new(0, -1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power.", this, new(0, 7), new(0, 1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(5, 0), new(1, 0)), "no", "outn");
                Pins.Add(new FixedOrientedPin("positiveoutput", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "outp", "output");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                if (Variants.Contains(_differentialInput))
                {
                    if (Variants.Contains(_swapInput))
                    {
                        SetPinOffset(0, new(-5, 4));
                        SetPinOffset(1, new(-5, -4));
                    }
                    else
                    {
                        SetPinOffset(0, new(-5, -4));
                        SetPinOffset(1, new(-5, 4));
                    }
                }
                else
                {
                    SetPinOffset(0, new(-5, 0));
                    SetPinOffset(1, new(-5, 0));
                }

                if (Variants.Contains(_differentialOutput))
                {
                    if (Variants.Contains(_swapOutput))
                    {
                        SetPinOffset(4, new(5, -4));
                        SetPinOffset(5, new(5, 4));
                    }
                    else
                    {
                        SetPinOffset(4, new(5, 4));
                        SetPinOffset(5, new(5, -4));
                    }
                }
                else
                {
                    SetPinOffset(4, new(5, 0));
                    SetPinOffset(5, new(5, 0));
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (Variants.Contains(_differentialInput))
                {
                    drawing.ExtendPins(Pins, 2, "inn", "inp");
                    if (Variants.Contains(_swapInput))
                        drawing.Signs(new(-2, -4), new(-2, 4));
                    else
                        drawing.Signs(new(-2, 4), new(-2, -4));
                }
                else
                    drawing.ExtendPin(Pins["n"]);

                if (Variants.Contains(_differentialOutput))
                {
                    drawing.ExtendPins(Pins, 3, "outp", "outn");
                    if (Variants.Contains(_swapOutput))
                        drawing.Signs(new(6, 7), new(6, -7));
                    else
                        drawing.Signs(new(6, -7), new(6, 7));
                }
                else
                    drawing.ExtendPin(Pins["o"]);

                // The triangle
                drawing.Polygon(new Vector2[] {
                    new(-5, -9),
                    new(5, -5),
                    new(5, 5),
                    new(-5, 9)
                });

                if (Variants.Contains(_programmable))
                    drawing.Arrow(new(-7, 10), new(6, -12));

                _anchors.Draw(drawing, Labels, this);
            }
        }
    }
}