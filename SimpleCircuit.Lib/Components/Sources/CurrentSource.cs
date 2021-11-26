using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    [SimpleKey("I", "A current source.", Category = "Sources")]
    public class CurrentSource : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the source.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public CurrentSource(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The current end point.", this, new(-8, 0), new(-1, 0)), "p", "b");
            Pins.Add(new FixedOrientedPin("negative", "The current starting point.", this, new(8, 0), new(1, 0)), "n", "a");

            DrawingVariants = Variant.Do(DrawSource);
        }

        /// <inheritdoc/>
        private void DrawSource(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new Vector2[]
            {
                new(-8, 0), new(-6, 0),
                new(6, 0), new(8, 0)
            }, new("wire"));

            // The circle with the arrow
            drawing.Circle(new(0, 0), 6);
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
        public override string ToString() => $"Current source {Name}";
    }
}