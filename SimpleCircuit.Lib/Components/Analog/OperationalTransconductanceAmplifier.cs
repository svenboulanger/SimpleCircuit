using SimpleCircuit.Components.Pins;
using System.Linq;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA).
    /// </summary>
    [SimpleKey("OTA", "A transconductance amplifier.", Category = "Analog")]
    [SimpleKey("TA", "A transconductance amplifier.", Category = "Analog")]
    public class OperationalTransconductanceAmplifier : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationalTransconductanceAmplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public OperationalTransconductanceAmplifier(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("negative", "The negative input.", this, new(-5, -4), new(-1, 0)), "n", "neg");
            Pins.Add(new FixedOrientedPin("positive", "The positive input.", this, new(-5, 4), new(-1, 0)), "p", "pos");
            Pins.Add(new FixedOrientedPin("output", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "output");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new[] {
                new Vector2(-5, -8),
                new Vector2(5, -4),
                new Vector2(5, 4),
                new Vector2(-5, 8)
            });
            drawing.Line(new(-3, -4), new(-1, -4), "minus");
            drawing.Segments(new Vector2[] {
                new(-2, 5), new(-2, 3),
                new(-3, 4), new(-1, 4)
            }, "plus");

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(5, 5), new Vector2(1, 1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"OTA {Name}";
    }
}
