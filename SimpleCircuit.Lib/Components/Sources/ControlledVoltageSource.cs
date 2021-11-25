using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A controlled voltage source.
    /// </summary>
    [SimpleKey("E", "A controlled voltage source.", Category = "Sources")]
    public class ControlledVoltageSource : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the source.")]
        public string Label { get; set; }

        /// <summary>
        /// Creates a new controlled voltage source.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        public ControlledVoltageSource(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-8, 0), new(-1, 0)), "n", "neg", "b");
            Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(8, 0), new(1, 0)), "p", "pos", "a");
        }

        protected override void Draw(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new Vector2[]
            {
                new(-8, 0), new(-6, 0),
                new(6, 0), new(8, 0)
            }, new("wire"));

            // Diamond
            drawing.Polygon(new Vector2[]
            {
                new(-6, 0), new(0, 6), new(6, 0), new(0, -6)
            });

            // Plus and minus
            drawing.Line(new(-3, -1), new(-3, 1), new("minus"));
            drawing.Segments(new Vector2[]
            {
                new(3, -1), new(3, 1),
                new(2, 0), new(4, 0)
            }, new("plus"));

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -8), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => $"Controlled voltage source {Name}";
    }
}
