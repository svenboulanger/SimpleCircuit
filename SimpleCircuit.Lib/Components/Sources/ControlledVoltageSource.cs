using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A controlled voltage source.
    /// </summary>
    [Drawable(new[] { "E", "H" }, "A controlled voltage source.", new[] { "Sources" })]
    public class ControlledVoltageSource : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the source.")]
            public string Label { get; set; }
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-8, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(8, 0), new(1, 0)), "p", "pos", "a");
                DrawingVariants = Variant.Do(DrawSource);
            }

            private void DrawSource(SvgDrawing drawing)
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
        }
    }
}