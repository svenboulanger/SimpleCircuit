using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A capacitor.
    /// </summary>
    [SimpleKey("C", "A capacitor.", Category = "Analog")]
    public class Capacitor : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the capacitor.")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets a flag that displays a polar capacitor.
        /// </summary>
        [Description("displays a polar capacitor.")]
        public bool Polar { get; set; }

        /// <inheritdoc />
        protected override IEnumerable<string> GroupClasses
        {
            get
            {
                if (Polar)
                    yield return "polar";
                else
                    yield return "apolar";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Capacitor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Capacitor(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("pos", "The positive pin", this, new(-5, 0), new(-1, 0)), "p", "pos", "a");
            Pins.Add(new FixedOrientedPin("neg", "the negative pin", this, new(5, 0), new(1, 0)), "n", "neg", "b");
            Polar = options?.PolarCapacitors ?? false;
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new Vector2[]
            {
                new(-5, 0), new(-1.5, 0),
                new(Polar ? 1 : 1.5, 0), new(5, 0)
            }, new("wire"));

            if (Polar)
            {
                drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos"));
                drawing.OpenBezier(new Vector2[]
                {
                    new(2.5, -4),
                    new(1, -2), new(1, -0.5), new(1, 0),
                    new(1, 0.5), new(1, 2), new(2.5, 4)
                }, new("neg"));

                // Add a little plus and minus next to the terminals!
                drawing.Segments(new Vector2[]
                {
                    new(-4, 2), new(-4, 4),
                    new(-5, 3), new(-3, 3)
                }, new("plus"));
                drawing.Line(new(5, 2), new(5, 4), new("minus"));
            }
            else
            {
                // Plates
                drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos"));
                drawing.Line(new(1.5, -4), new(1.5, 4), new("neg"));
            }

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Capacitor {Name}";
    }
}
