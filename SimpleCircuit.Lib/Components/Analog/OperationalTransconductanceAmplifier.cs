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
        public override IDrawable Create(string key, string name, Options options)
        {
            var result = new Instance(name, options);

            // We will just add a differential input as the default
            result.AddVariant("diffin");
            return result;
        }

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the OTA.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "ota";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative input.", this, new(-5, -4), new(-1, 0)), "n", "neg");
                Pins.Add(new FixedOrientedPin("positive", "The positive input.", this, new(-5, 4), new(-1, 0)), "p", "pos");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power.", this, new(0, -7), new(0, -1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power.", this, new(0, 7), new(0, 1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(5, 0), new(1, 0)), "no", "outn");
                Pins.Add(new FixedOrientedPin("positiveoutput", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "output");

                PinUpdate = Variant.All(
                    Variant.Map(_differentialInput, _swapInput, RedefineInput),
                    Variant.Map(_differentialOutput, _swapOutput, RedefineOutput));
                DrawingVariants = Variant.All(
                    Variant.Do(DrawOTA),
                    Variant.If(_differentialInput).Then(Variant.Map(_swapInput, DrawInputSigns)),
                    Variant.If(_differentialOutput).Then(Variant.Map(_swapOutput, DrawOutputSigns)),
                    Variant.If(_programmable).Then(DrawProgrammable));
            }

            private void DrawOTA(SvgDrawing drawing)
            {
                drawing.ExtendPin(Pins["n"]);
                drawing.ExtendPin(Pins["o"], HasVariant(_differentialOutput) ? 3 : 2);

                // The triangle
                drawing.Polygon(new Vector2[] {
                    new(-5, -9),
                    new(5, -5),
                    new(5, 5),
                    new(-5, 9)
                });

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(5, 5), new Vector2(1, 1));
            }

            private void DrawInputSigns(SvgDrawing drawing, bool swapped)
            {
                drawing.ExtendPin(Pins["p"]);
                if (swapped)
                    drawing.Signs(new(-2, -4), new(-2, 4));
                else
                    drawing.Signs(new(-2, 4), new(-2, -4));
            }
            private void DrawOutputSigns(SvgDrawing drawing, bool swapped)
            {
                drawing.ExtendPin(Pins["no"], 3);

                if (swapped)
                    drawing.Signs(new(6, 7), new(6, -7));
                else
                    drawing.Signs(new(6, -7), new(6, 7));
            }
            private void DrawProgrammable(SvgDrawing drawing)
            {
                drawing.Arrow(new(-7, 10), new(6, -12));
            }

            private void RedefineInput(bool differential, bool swapped)
            {
                if (differential)
                {
                    if (swapped)
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
            }
            private void RedefineOutput(bool differential, bool swapped)
            {
                if (differential)
                {
                    if (swapped)
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
            }
        }
    }
}