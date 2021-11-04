using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    [SimpleKey("I", "A current source", Category = "Analog")]
    public class CurrentSource : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CurrentSource(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("positive", "The current end point.", this, new(-8, 0), new(-1, 0)), "p", "b");
            Pins.Add(new FixedOrientedPin("negative", "The current starting point.", this, new(8, 0), new(1, 0)), "n", "a");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Circle(new Vector2(0, 0), 6);
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(-3, 0), new Vector2(3, 0),
                new Vector2(3, 0), new Vector2(1, 2),
                new Vector2(3, 0), new Vector2(1, -2),
                new Vector2(6, 0), new Vector2(8, 0)
            });

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -8), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Current source {Name}";
    }
}