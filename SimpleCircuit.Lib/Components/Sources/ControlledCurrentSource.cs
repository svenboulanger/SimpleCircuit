using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A controlled current source.
    /// </summary>
    [Drawable(new[] { "G", "F" }, "A controlled current source.", new[] { "Sources" })]
    public class ControlledCurrentSource : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the source.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "ccs";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The current end point.", this, new(-6, 0), new(-1, 0)), "p", "b");
                Pins.Add(new FixedOrientedPin("negative", "The current starting point.", this, new(6, 0), new(1, 0)), "n", "a");
            }

            /// <inheritdoc/>
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // Diamond
                drawing.Polygon(new Vector2[]
                {
                    new(-6, 0), new(0, 6), new(6, 0), new(0, -6)
                });

                // The circle with the arrow
                drawing.Line(new(-3, 0), new(3, 0), new("arrow") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });

                // Depending on the orientation, let's anchor the text differently
                drawing.Text(Label, new(0, -8), new(0, -1));
            }
        }
    }
}