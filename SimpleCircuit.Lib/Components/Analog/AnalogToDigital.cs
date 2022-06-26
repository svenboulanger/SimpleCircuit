using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An analog-to-digital converter.
    /// </summary>
    [Drawable("ADC", "An analog-to-digital converter.", "Digital")]
    public class AnalogToDigital : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

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

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, new(-9, 0), new(-1, 0)), "input", "in", "pi", "inp");
                Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, new(-9, 0), new(-1, 0)), "inn", "ni");
                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(9, 0), new(1, 0)), "outn", "no");
                Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, new(9, 0), new(1, 0)), "output", "out", "po", "outp");
                Variants.Changed += UpdatePins;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (Variants.Contains(_differentialInput))
                {
                    drawing.ExtendPins(Pins, 2, "inp", "inn");
                    double x = -Width / 2 + 3;
                    double y = Height / 4;
                    drawing.Signs(new(x, -y), new(x, y));
                }
                else
                    drawing.ExtendPin(Pins["in"]);

                if (Variants.Contains(_differentialOutput))
                {
                    drawing.ExtendPins(Pins, 4, "outp", "outn");
                    double x = Width / 2 - Height / 4 + 2;
                    double y = Height / 4 + 1.5;
                    drawing.Signs(new(x, -y), new(x, y));
                }
                else
                    drawing.ExtendPin(Pins["out"]);

                drawing.Polygon(new[]
                {
                    new Vector2(-Width / 2, Height / 2), new Vector2(Width / 2 - Height / 2, Height / 2),
                    new Vector2(Width / 2, 0), new Vector2(Width / 2 - Height / 2, -Height / 2),
                    new Vector2(-Width / 2, -Height / 2)
                });

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(-Height / 4, 0), new Vector2(0, 0));
            }
            private void UpdatePins(object sender, EventArgs e)
            {
                double x = -Width / 2;
                double y = Height / 4;
                if (Variants.Contains(_differentialInput))
                {
                    if (Variants.Contains(_swapInput))
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

                if (Variants.Contains(_differentialOutput))
                {
                    x = Width / 2 - Height / 4;
                    if (Variants.Contains(_swapOutput))
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
                    x = Width / 2;
                    SetPinOffset(2, new(x, 0));
                    SetPinOffset(3, new(x, 0));
                }
            }
        }
    }
}