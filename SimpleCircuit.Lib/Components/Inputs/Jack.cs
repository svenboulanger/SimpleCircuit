using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Inputs
{
    /// <summary>
    /// A jack.
    /// </summary>
    [Drawable("JACK", "A (phone) jack.", "Inputs")]
    public class Jack : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("Adds a label next to the jack.")]
            public string Label { get; set; }
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(0, 6), new(0, 1)), "p", "a", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b", "neg");
                DrawingVariants = Variant.Do(DrawJack);
            }
            private void DrawJack(SvgDrawing drawing)
            {
                drawing.Segments(new Vector2[]
                {
                    new(0, 2), new(0, 6),
                    new(4, 0), new(6, 0)
                }, new("wire"));

                drawing.Circle(new(), 1.5);
                drawing.Circle(new(), 4);
                drawing.Circle(new(4, 0), 1, new("dot"));

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(-6, 0), new(-1, 0));
            }
        }
    }
}