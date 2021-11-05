using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A supply voltage.
    /// </summary>
    [SimpleKey("POW", "Power plane.", Category = "General")]
    public class Power : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The power plane name.")]
        public string Label { get; set; } = "VDD";

        /// <summary>
        /// Initializes a new instance of the <see cref="Ground"/> class.
        /// </summary>
        /// <param name="options">Options that can be used for the component.</param>
        public Power(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, 1)), "x", "p", "a");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Line(new Vector2(0, 0), new Vector2(0, -3));
            drawing.Line(new Vector2(-5, -3), new Vector2(5, -3), "plane");
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Power {Name}";
    }
}
