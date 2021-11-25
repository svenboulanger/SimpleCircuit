using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using System;
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
            new(-8, -4), new(-8, 4), new(8, -4), new(8, 4)
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
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new Vector2[]
            {
                new(-8, -8),
                new(8, 0),
                new(-8, 8)
            });

            // Draw plus and minus for the inputs
            if (Variants.Contains("diffin"))
            {
                drawing.Segments(new Vector2[]
                {
                    new(-6, -4), new(-4, -4),
                    new(-5, 5), new(-5, 3),
                }.Select(v => Variants.Contains("swapin") ? new Vector2(v.X, v.Y) : new Vector2(v.X, -v.Y)), new("plus"));
                drawing.Segments(new Vector2[] {
                    new(-6, 4), new(-4, 4)
                }.Select(v => Variants.Contains("swapin") ? new Vector2(v.X, v.Y) : new Vector2(v.X, -v.Y)), new("minus"));
            }

            if (Variants.Contains("diffout"))
            {
                drawing.Segments(new Vector2[]
{
                    new(6, -6), new(4, -6),
                    new(5, 7), new(5, 5),
                }.Select(v => Variants.Contains("swapout") ? new Vector2(v.X, v.Y) : new Vector2(v.X, -v.Y)), new("plus"));
                drawing.Segments(new Vector2[] {
                    new(6, 6), new(4, 6)
                }.Select(v => Variants.Contains("swapout") ? new Vector2(v.X, -v.Y) : new Vector2(v.X, v.Y)), new("minus"));

                drawing.Segments(new Vector2[]
                {
                    new(0, 4), new(8, 4),
                    new(0, -4), new(8, -4)
                }, new("wire"));
            }

            if (Variants.Contains("programmable"))
            {
                var options = new PathOptions() { EndMarker = PathOptions.MarkerTypes.Arrow };
                drawing.Polyline(new Vector2[] { new(-7, 10), new(4, -8.5) }, options);
            }

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, new(-2.5, 0), new());
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            // Inputs
            var pin1 = (FixedOrientedPin)Pins[0];
            var pin2 = (FixedOrientedPin)Pins[1];
            if (Variants.Contains("diffin"))
            {
                if (Variants.Contains("swapin"))
                {
                    pin1.Offset = _pinOffsets[1];
                    pin2.Offset = _pinOffsets[0];
                }
                else
                {
                    pin1.Offset = _pinOffsets[0];
                    pin2.Offset = _pinOffsets[1];
                }
            }
            else
            {
                var offset = (_pinOffsets[0] + _pinOffsets[1]) / 2.0;
                pin1.Offset = offset;
                pin2.Offset = offset;
            }

            // Outputs
            pin1 = (FixedOrientedPin)Pins[2];
            pin2 = (FixedOrientedPin)Pins[3];
            if (Variants.Contains("diffout"))
            {
                if (Variants.Contains("swapout"))
                {
                    pin1.Offset = _pinOffsets[3];
                    pin2.Offset = _pinOffsets[2];
                }
                else
                {
                    pin1.Offset = _pinOffsets[2];
                    pin2.Offset = _pinOffsets[3];
                }
            }
            else
            {
                var offset = (_pinOffsets[2] + _pinOffsets[3]) / 2.0;
                pin1.Offset = offset;
                pin2.Offset = offset;
            }

            base.DiscoverNodeRelationships(context, diagnostics);
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
