using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An impedance/admittance.
    /// </summary>
    [SimpleKey("Z", "An impedance", Category = "Analog")]
    [SimpleKey("Y", "An admittance", Category = "Analog")]
    public class Impedance : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Impedance"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Impedance(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-8, 0), new(-1, 0)), "p", "pos", "a");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(8, 0), new(1, 0)), "n", "neg", "b");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 0), new Vector2(8, 0)
            });
            drawing.Polygon(new[] { new Vector2(-6, 3), new Vector2(6, 3), new Vector2(6, -3), new Vector2(-6, -3) });

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Impedance {Name}";

    }
}
