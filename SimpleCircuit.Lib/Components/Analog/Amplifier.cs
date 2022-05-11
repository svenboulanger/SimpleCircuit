using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System.Linq;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A factory for amplifiers.
    /// </summary>
    [Drawable("A", "A generic amplifier.", "Analog")]
    public class Amplifier : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private static readonly Vector2[] _pinOffsets = new Vector2[] {
                new(-8, -4), new(-8, 4), new(-2, -5), new(-2, 5), new(8, 4), new(8, -4)
            };

            public string Label { get; set; }
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, _pinOffsets[0], new(-1, 0)), "i", "in", "inp", "pi", "p");
                Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, _pinOffsets[1], new(-1, 0)), "inn", "ni", "n");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power supply.", this, _pinOffsets[2], new(0, -1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power supply.", this, _pinOffsets[3], new(0, 1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, _pinOffsets[4], new(1, 0)), "outn", "no");
                Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, _pinOffsets[5], new(1, 0)), "o", "out", "outp", "po");

                // Resolving pins
                PinUpdate = Variant.All(
                    Variant.Map("diffin", "swapin", (b1, b2) => RedefinePins(0, b1, b2)),
                    Variant.Map("diffout", "swapout", (b1, b2) => RedefinePins(4, b1, b2))
                );
                DrawingVariants = Variant.All(
                    Variant.If("diffin").Do(Variant.Map("swapin", DrawDifferentialInput)),
                    Variant.If("diffout").Do(Variant.Map("swapout", DrawDifferentialOutput)),
                    Variant.Do(DrawAmplifier),
                    Variant.If("programmable").Do(DrawProgrammable));
            }
            private void DrawDifferentialInput(SvgDrawing drawing, bool swapped)
            {
                var modifier = (Vector2 v) => swapped ? new Vector2(v.X, -v.Y) : v;
                drawing.Path(b => b.WithModifier(modifier).MoveTo(-6, -4).Line(2, 0).MoveTo(-5, -5).Line(0, 2), new("plus"));
                drawing.Line(modifier(new(-6, 4)), modifier(new(-4, 4)), new("minus"));
            }
            private void DrawDifferentialOutput(SvgDrawing drawing, bool swapped)
            {
                var modifier = (Vector2 v) => swapped ? new Vector2(v.X, -v.Y) : v;
                drawing.Path(b => b.MoveTo(0, 4).Line(8, 0).MoveTo(0, -4).Line(8, 0), new("wire"));
                drawing.Path(b => b.WithModifier(modifier).MoveTo(4, -6).Line(2, 0).MoveTo(5, -7).Line(0, 2), new("plus"));
                drawing.Line(modifier(new(4, 6)), modifier(new(6, 6)), new("minus"));
            }
            private void DrawProgrammable(SvgDrawing drawing)
            {
                var options = new PathOptions() { EndMarker = PathOptions.MarkerTypes.Arrow };
                drawing.Line(new(-7, 10), new(4, -8.5), options);
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
            private void RedefinePins(int p1, bool differential, bool swapped)
            {
                int p2 = p1 + 1;
                if (differential)
                {
                    if (swapped)
                    {
                        SetPinOffset(p1, _pinOffsets[p2]);
                        SetPinOffset(p2, _pinOffsets[p1]);
                    }
                    else
                    {
                        SetPinOffset(p1, _pinOffsets[p1]);
                        SetPinOffset(p2, _pinOffsets[p2]);
                    }
                }
                else
                {
                    var offset = (_pinOffsets[p1] + _pinOffsets[p2]) / 2.0;
                    SetPinOffset(p1, offset);
                    SetPinOffset(p2, offset);
                }
            }
        }
    }
}
