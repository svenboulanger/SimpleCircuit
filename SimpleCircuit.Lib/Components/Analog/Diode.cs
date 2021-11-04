using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A diode.
    /// </summary>
    [SimpleKey("D", "A diode", Category = "Analog")]
    public class Diode : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Diode"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Diode(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("anode", "The anode.", this, new(-6, 0), new(-1, 0)), "p", "a", "anode");
            Pins.Add(new FixedOrientedPin("cathode", "The cathode.", this, new(6, 0), new(1, 0)), "n", "c", "cathode");
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(-6, 0), new Vector2(-4, 0),
                new Vector2(4, -4), new Vector2(4, 4),
                new Vector2(4, 0), new Vector2(6, 0)
            });
            drawing.Polygon(new[] {
                new Vector2(-4, -4),
                new Vector2(4, 0),
                new Vector2(-4, 4),
                new Vector2(-4, 4)
            });

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Diode {Name}";
    }
}
