using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An analog-to-digital converter.
    /// </summary>
    [Drawable("ADC", "An analog-to-digital converter.", "Digital")]
    public class AnalogToDigital : DrawableFactory
    {
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private const string _differentialInput = "diffin";
            private const string _swapInput = "swapin";
            private const string _differentialOutput = "diffout";
            private const string _swapOutput = "swapout";

            [Description("The width of the ADC.")]
            public double Width { get; set; } = 18;

            [Description("The height of the ADC.")]
            public double Height { get; set; } = 12;

            [Description("The label inside the ADC.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "adc";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, new(-9, 0), new(-1, 0)), "input", "in", "pi", "inp");
                Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, new(-9, 0), new(-1, 0)), "inn", "ni");
                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(9, 0), new(1, 0)), "outn", "no");
                Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, new(9, 0), new(1, 0)), "output", "out", "po", "outp");

                PinUpdate = Variant.All(
                    Variant.Map(_differentialInput, _swapInput, RedefineInputPins),
                    Variant.Map(_differentialOutput, _swapOutput, RedefineOutputPins));
                DrawingVariants = Variant.All(
                    Variant.Do(DrawADC),
                    Variant.If(_differentialInput).Then(DrawDifferentialInput),
                    Variant.If(_differentialOutput).Then(DrawOutputSigns));
            }

            private void DrawADC(SvgDrawing drawing)
            {
                if (Pins[0].Connections == 0)
                    drawing.ExtendPin(Pins[0]);
                if (Pins[3].Connections == 0)
                    drawing.ExtendPin(Pins[3], HasVariant(_differentialOutput) ? 4 : 2 );

                drawing.Polygon(new[]
                {
                    new Vector2(-Width / 2, Height / 2), new Vector2(Width / 2 - Height / 2, Height / 2),
                    new Vector2(Width / 2, 0), new Vector2(Width / 2 - Height / 2, -Height / 2),
                    new Vector2(-Width / 2, -Height / 2)
                });

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(-Height / 4, 0), new Vector2(0, 0));
            }
            private void DrawDifferentialInput(SvgDrawing drawing)
            {
                if (Pins[1].Connections == 0)
                    drawing.ExtendPin(Pins[1]);

                double x = -Width / 2 + 3;
                double y = Height / 4;
                drawing.Signs(new(x, -y), new(x, y));
            }
            private void DrawOutputSigns(SvgDrawing drawing)
            {
                if (Pins[2].Connections == 0)
                    drawing.ExtendPin(Pins[2], 4);

                if (Pins[3].Connections == 0)
                {
                    var loc = ((FixedOrientedPin)Pins[3]).Offset;
                    drawing.Line(loc, loc + new Vector2(4, 0), new("wire"));
                }

                double x = Width / 2 - Height / 4 + 2;
                double y = Height / 4 + 1.5;
                drawing.Signs(new(x, -y), new(x, y));
            }
            private void RedefineInputPins(bool differential, bool swap)
            {
                double x = -Width / 2;
                if (differential)
                {
                    double y = Height / 4;
                    if (swap)
                    {
                        SetPinOffset(0, new(x, y));
                        SetPinOffset(1, new(x, -y));
                    }
                    else
                    {
                        SetPinOffset(0, new(x, -y));
                        SetPinOffset(1, new(x, y));
                    }
                }
                else
                {
                    SetPinOffset(0, new(x, 0));
                    SetPinOffset(1, new(x, 0));
                }
            }
            private void RedefineOutputPins(bool differential, bool swap)
            {
                if (differential)
                {
                    double x = Width / 2 - Height / 4;
                    double y = Height / 4;
                    if (swap)
                    {
                        SetPinOffset(2, new(x, -y));
                        SetPinOffset(3, new(x, y));
                    }
                    else
                    {
                        SetPinOffset(2, new(x, y));
                        SetPinOffset(3, new(x, -y));
                    }
                }
                else
                {
                    double x = Width / 2;
                    SetPinOffset(2, new(x, 0));
                    SetPinOffset(3, new(x, 0));
                }
            }
        }
    }
}