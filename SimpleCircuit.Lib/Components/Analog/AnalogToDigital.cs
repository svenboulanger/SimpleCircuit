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
                    Variant.Map("diffin", RedefineInputPins),
                    Variant.Map("diffout", RedefineOutputPins));
                DrawingVariants = Variant.All(
                    Variant.Do(DrawADC),
                    Variant.If("diffin").Then(DrawInputSigns),
                    Variant.If("diffout").Then(DrawOutputSigns));
            }

            private void DrawADC(SvgDrawing drawing)
            {
                drawing.Polygon(new[]
                {
                    new Vector2(-Width / 2, Height / 2), new Vector2(Width / 2 - Height / 2, Height / 2),
                    new Vector2(Width / 2, 0), new Vector2(Width / 2 - Height / 2, -Height / 2),
                    new Vector2(-Width / 2, -Height / 2)
                });

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(-Height / 4, 0), new Vector2(0, 0));
            }
            private void DrawInputSigns(SvgDrawing drawing)
            {
                double x = -Width / 2 + 3;
                double y = Height / 4;
                drawing.Signs(new(x, -y), new(x, y));
            }
            private void DrawOutputSigns(SvgDrawing drawing)
            {
                if (Pins[2].Connections == 0)
                {
                    var loc = ((FixedOrientedPin)Pins[2]).Offset;
                    drawing.Line(loc, loc + new Vector2(4, 0), new("wire"));
                }
                if (Pins[3].Connections == 0)
                {
                    var loc = ((FixedOrientedPin)Pins[3]).Offset;
                    drawing.Line(loc, loc + new Vector2(4, 0), new("wire"));
                }

                double x = Width / 2 - Height / 4 + 2;
                double y = Height / 4 + 1.5;
                drawing.Signs(new(x, -y), new(x, y));
            }
            private void SetPinOffset(int index, Vector2 offset)
                => ((FixedOrientedPin)Pins[index]).Offset = offset;
            private void RedefineInputPins(bool differential)
            {
                SetPinOffset(0, differential ? new(-Width / 2, -Height / 4) : new(-Width / 2, 0));
                SetPinOffset(1, differential ? new(-Width / 2, Height / 4) : new(-Width / 2, 0));
            }
            private void RedefineOutputPins(bool differential)
            {
                SetPinOffset(2, differential ? new(Width / 2 - Height / 4, Height / 4) : new(Width / 2, 0));
                SetPinOffset(3, differential ? new(Width / 2 - Height / 4, -Height / 4) : new(Width / 2, 0));
            }
        }
    }
}