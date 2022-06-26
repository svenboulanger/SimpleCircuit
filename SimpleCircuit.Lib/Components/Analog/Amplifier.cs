using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A factory for amplifiers.
    /// </summary>
    [Drawable("A", "A generic amplifier.", "Analog")]
    [Drawable("OA", "An operational amplifier.", "Analog")]
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

        private class Instance : ScaledOrientedDrawable, ILabeled
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


            /// <inheritdoc />
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the gain string.
            /// </summary>
            public string Gain { get; set; }

            /// <inheritdoc />
            public override string Type => "amplifier";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name of the instance.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, _inputCommon, new(-1, 0)), "i", "in", "inp", "pi", "p");
                Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, _inputCommon, new(-1, 0)), "inn", "ni", "n");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power supply.", this, _supplyPos, new(0, -1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power supply.", this, _supplyNeg, new(0, 1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, _outputCommon, new(1, 0)), "outn", "no");
                Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, _outputCommon, new(1, 0)), "o", "out", "outp", "po");
                Variants.Changed += UpdatePins;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                // Differential input?
                if (Variants.Contains(_differentialInput))
                {
                    drawing.ExtendPins(Pins, 2, "inp", "inn");
                    if (Variants.Contains(_swapInput))
                        drawing.Signs(new(-5.5, 4), new(-5.5, -4));
                    else
                        drawing.Signs(new(-5.5, -4), new(-5.5, 4));
                }
                else
                    drawing.ExtendPin(Pins["in"]);

                // Differential output?
                if (Variants.Contains(_differentialOutput))
                {
                    drawing.ExtendPins(Pins, 5, "outp", "outn");
                    if (Variants.Contains(_swapOutput))
                        drawing.Signs(new(6, -7), new(6, 7));
                    else
                        drawing.Signs(new(6, 7), new(6, -7));
                }
                else
                    drawing.ExtendPin(Pins["out"]);

                drawing.Polygon(new Vector2[]
                {
                    new(-8, -8),
                    new(8, 0),
                    new(-8, 8)
                });

                // Programmable arrow
                if (Variants.Contains(_programmable))
                    drawing.Arrow(new(-7, 10), new(4, -8.5));

                // Comparator
                if (Variants.Contains(_comparator))
                {
                    drawing.Path(b =>
                    {
                        b.MoveTo(-4, 2)
                        .LineTo(-2, 2)
                        .LineTo(-2, -2)
                        .LineTo(0, -2);
                    });
                }

                // Schmitt trigger
                if (Variants.Contains(_schmitt))
                {
                    drawing.Path(b =>
                    {
                        b.MoveTo(-5, 2)
                            .LineTo(-3, 2)
                            .LineTo(-3, -2)
                            .LineTo(-1, -2);
                        b.MoveTo(-3, 2)
                            .LineTo(-1, 2)
                            .LineTo(-1, -2)
                            .LineTo(1, -2);
                    });
                }

                // Labels
                if (!string.IsNullOrWhiteSpace(Label))
                {
                    if (Variants.Contains(_differentialOutput))
                        drawing.Text(Label, new(2, 7), new(1, 1));
                    else
                        drawing.Text(Label, new(2, 5), new(1, 1));
                }
                if (!string.IsNullOrWhiteSpace(Gain))
                    drawing.Text(Gain, new(-2.5, 0), new());
            }
            private void UpdatePins(object sender, EventArgs e)
            {
                if (Variants.Contains(_differentialInput))
                {
                    if (Variants.Contains(_swapInput))
                    {
                        SetPinOffset(0, _inputNeg);
                        SetPinOffset(1, _inputPos);
                    }
                    else
                    {
                        SetPinOffset(0, _inputPos);
                        SetPinOffset(1, _inputNeg);
                    }
                }
                else
                {
                    SetPinOffset(0, _inputCommon);
                    SetPinOffset(1, _inputCommon);
                }

                if (Variants.Contains(_differentialOutput))
                {
                    if (Variants.Contains(_differentialInput))
                    {
                        SetPinOffset(4, _outputPos);
                        SetPinOffset(5, _outputNeg);
                    }
                    else
                    {
                        SetPinOffset(4, _outputNeg);
                        SetPinOffset(5, _outputPos);
                    }
                }
                else
                {
                    SetPinOffset(4, _outputCommon);
                    SetPinOffset(5, _outputCommon);
                }
            }
        }
    }
}
