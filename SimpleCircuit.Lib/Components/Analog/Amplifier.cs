using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System.Linq;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An amplifier.
    /// </summary>
    [SimpleKey("A", "A generic amplifier.", Category = "Analog")]
    public class Amplifier : ScaledOrientedDrawable, ILabeled
    {
        private static readonly Vector2[] _pinOffsets = new Vector2[] {
            new(-8, -4), new(-8, 4), new(8, 4), new(8, -4)
        };

        /// <inheritdoc/>
        [Description("The label in the amplifier.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Amplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Amplifier(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, _pinOffsets[0], new(-1, 0)), "i", "in", "inp", "pi", "p");
            Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, _pinOffsets[1], new(-1, 0)), "inn", "ni", "n");
            Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, _pinOffsets[2], new(1, 0)), "outn", "no");
            Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, _pinOffsets[3], new(1, 0)), "o", "out", "outp", "po");

            // Resolving pins
            PinUpdate = Variant.All(
                Variant.Map("diffin", "swapin", (b1, b2) => RedefinePins(0, b1, b2)),
                Variant.Map("diffout", "swapout", (b1, b2) => RedefinePins(2, b1, b2))
            );
            DrawingVariants = Variant.All(
                Variant.If("diffin").Do(Variant.Map("swapin", DrawDifferentialInput)),
                Variant.If("diffout").Do(Variant.Map("swapout", DrawDifferentialOutput)),
                Variant.Do(DrawAmplifier),
                Variant.If("programmable").Do(DrawProgrammable));
        }
        private void DrawDifferentialInput(SvgDrawing drawing, bool swapped)
        {
            drawing.Segments(new Vector2[] {
                new(-6, -4), new(-4, -4),
                new(-5, -5), new(-5, -3),
            }.Select(v => swapped ? new Vector2(v.X, -v.Y) : new Vector2(v.X, v.Y)), new("plus"));
            drawing.Segments(new Vector2[] {
                new(-6, 4), new(-4, 4)
            }.Select(v => swapped ? new Vector2(v.X, -v.Y) : new Vector2(v.X, v.Y)), new("minus"));
        }
        private void DrawDifferentialOutput(SvgDrawing drawing, bool swapped)
        {
            drawing.Segments(new Vector2[] {
                new(0, 4), new(8, 4),
                new(0, -4), new(8, -4)
            }, new("wire"));
            drawing.Segments(new Vector2[] {
                new(4, -6), new(6, -6),
                new(5, -7), new(5, -5),
            }.Select(v => swapped ? new Vector2(v.X, -v.Y) : new Vector2(v.X, v.Y)), new("plus"));
            drawing.Segments(new Vector2[] {
                new(4, 6), new(6, 6)
            }.Select(v => swapped ? new Vector2(v.X, -v.Y) : new Vector2(v.X, v.Y)), new("minus"));
        }
        private void DrawProgrammable(SvgDrawing drawing)
        {
            var options = new PathOptions() { EndMarker = PathOptions.MarkerTypes.Arrow };
            drawing.Polyline(new Vector2[] { new(-7, 10), new(4, -8.5) }, options);
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

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Amplifier {Name}";
    }
}
