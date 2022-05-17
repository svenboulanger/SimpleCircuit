using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA).
    /// </summary>
    [Drawable(new[] { "OTA", "TA" }, "A transconductance amplifier.", new[] { "Analog" })]
    public class OperationalTransconductanceAmplifier : DrawableFactory
    {
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
                    Variant.Map("diffin", "swapin", RedefineInput),
                    Variant.Map("diffout", "swapout", RedefineOutput));
                DrawingVariants = Variant.All(
                    Variant.Do(DrawOTA),
                    Variant.If("diffin").Then(Variant.Map("swapin", DrawInputSigns)),
                    Variant.If("diffout").Then(Variant.Map("swapout", DrawOutputSigns)),
                    Variant.If("programmable").Then(DrawProgrammable));
            }

            private void DrawOTA(SvgDrawing drawing)
            {
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
                if (swapped)
                    drawing.Signs(new(-2, -4), new(-2, 4));
                else
                    drawing.Signs(new(-2, 4), new(-2, -4));
            }
            private void DrawOutputSigns(SvgDrawing drawing, bool swapped)
            {
                if (Pins[4].Connections == 0)
                {
                    var loc = ((FixedOrientedPin)Pins[4]).Offset;
                    drawing.Line(loc, loc + new Vector2(3, 0), new("wire"));
                }
                if (Pins[5].Connections == 0)
                {
                    var loc = ((FixedOrientedPin)Pins[5]).Offset;
                    drawing.Line(loc, loc + new Vector2(3, 0), new("wire"));
                }

                if (swapped)
                    drawing.Signs(new(6, 7), new(6, -7));
                else
                    drawing.Signs(new(6, -7), new(6, 7));
            }
            private void DrawProgrammable(SvgDrawing drawing)
            {
                drawing.Arrow(new(-7, 10), new(6, -12));
            }

            private void SetPin(int index, Vector2 offset)
            {
                ((FixedOrientedPin)Pins[index]).Offset = offset;
            }
            private void RedefineInput(bool differential, bool swapped)
            {
                if (differential)
                {
                    if (swapped)
                    {
                        SetPin(0, new(-5, 4));
                        SetPin(1, new(-5, -4));
                    }
                    else
                    {
                        SetPin(0, new(-5, -4));
                        SetPin(1, new(-5, 4));
                    }
                }
                else
                {
                    SetPin(0, new(-5, 0));
                    SetPin(1, new(-5, 0));
                }
            }
            private void RedefineOutput(bool differential, bool swapped)
            {
                if (differential)
                {
                    if (swapped)
                    {
                        SetPin(4, new(5, -4));
                        SetPin(5, new(5, 4));
                    }
                    else
                    {
                        SetPin(4, new(5, 4));
                        SetPin(5, new(5, -4));
                    }
                }
                else
                {
                    SetPin(4, new(5, 0));
                    SetPin(5, new(5, 0));
                }
            }
        }
    }
}