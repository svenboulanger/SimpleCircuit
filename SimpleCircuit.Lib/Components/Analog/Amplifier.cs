using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;
using System.Linq;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A factory for amplifiers.
    /// </summary>
    [Drawable("A", "A generic amplifier.", "Analog")]
    [Drawable("OA", "An operational amplifier.", "Analog")]
    public class Amplifier : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            var result = new Instance(name, options);
            switch (key)
            {
                case "OA":
                    result.AddVariant("diffin");
                    break;
            }
            return result;
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

            /// <inheritdoc />
            public override string Type => "amplifier";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, _inputCommon, new(-1, 0)), "i", "in", "inp", "pi", "p");
                Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, _inputCommon, new(-1, 0)), "inn", "ni", "n");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power supply.", this, _supplyPos, new(0, -1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power supply.", this, _supplyNeg, new(0, 1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, _outputCommon, new(1, 0)), "outn", "no");
                Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, _outputCommon, new(1, 0)), "o", "out", "outp", "po");

                // Resolving pins
                PinUpdate = Variant.All(
                    Variant.Map("diffin", "swapin", (b1, b2) => RedefineInput(b1, b2)),
                    Variant.Map("diffout", "swapout", (b1, b2) => RedefineOutput(b1, b2))
                );
                DrawingVariants = Variant.All(
                    Variant.If("diffin").Then(Variant.Map("swapin", DrawDifferentialInput)),
                    Variant.If("diffout").Then(Variant.Map("swapout", DrawDifferentialOutput)),
                    Variant.If("schmitt").Then(DrawSchmitt).Else(Variant.If("comparator").Then(DrawComparator)),
                    Variant.Do(DrawAmplifier),
                    Variant.If("programmable").Then(DrawProgrammable));
            }
            private void DrawDifferentialInput(SvgDrawing drawing, bool swapped)
            {
                if (swapped)
                    drawing.Signs(new(-5, 4), new(-5, -4));
                else
                    drawing.Signs(new(-5, -4), new(-5, 4));
            }
            private void DrawDifferentialOutput(SvgDrawing drawing, bool swapped)
            {
                if (Pins[4].Connections == 0)
                {
                    var loc = ((FixedOrientedPin)Pins[4]).Offset;
                    drawing.Line(loc, loc + new Vector2(5, 0), new("wire"));
                }
                if (Pins[5].Connections == 0)
                {
                    var loc = ((FixedOrientedPin)Pins[5]).Offset;
                    drawing.Line(loc, loc + new Vector2(5, 0), new("wire"));
                }

                if (swapped)
                    drawing.Signs(new(5, -6), new(5, 6));
                else
                    drawing.Signs(new(5, 6), new(5, -6));
            }
            private void DrawProgrammable(SvgDrawing drawing)
                => drawing.Arrow(new(-7, 10), new(4, -8.5));
            private void DrawComparator(SvgDrawing drawing)
            {
                drawing.Path(b =>
                {
                    b.MoveTo(-4, 2)
                    .LineTo(-2, 2)
                    .LineTo(-2, -2)
                    .LineTo(0, -2);
                });
            }
            private void DrawSchmitt(SvgDrawing drawing)
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
            private void DrawAmplifier(SvgDrawing drawing)
            {
                drawing.Polygon(new Vector2[]
                {
                    new(-8, -8),
                    new(8, 0),
                    new(-8, 8)
                });
                if (!string.IsNullOrEmpty(Label))
                    drawing.Text(Label, new(-2.5, 0), new());
            }
            private void SetPinOffset(int index, Vector2 offset)
                => ((FixedOrientedPin)Pins[index]).Offset = offset;
            private void RedefineInput(bool differential, bool swapped)
            {
                if (differential)
                {
                    if (swapped)
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
            }
            private void RedefineOutput(bool differential, bool swapped)
            {
                if (differential)
                {
                    if (swapped)
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
