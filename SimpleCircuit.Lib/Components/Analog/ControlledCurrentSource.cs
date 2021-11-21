using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A controlled current source.
    /// </summary>
    [SimpleKey("G", "A controlled current source.", Category = "Analog")]
    public class ControlledCurrentSource : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the source.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public ControlledCurrentSource(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The current end point.", this, new(-8, 0), new(-1, 0)), "p", "b");
            Pins.Add(new FixedOrientedPin("negative", "The current starting point.", this, new(8, 0), new(1, 0)), "n", "a");
        }

        /// <inheritdoc/>
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

            // The circle with the arrow
            drawing.Line(new(-3, 0), new(3, 0), new("arrow") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new(0, -8), new(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Controlled current source {Name}";
    }
}
