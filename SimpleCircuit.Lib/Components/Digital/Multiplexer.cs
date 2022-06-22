using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A multiplexer.
    /// </summary>
    [Drawable("MUX", "A multiplexer.", "Digital")]
    public class Multiplexer : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the MUX.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "mux";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("1", "The '1' input.", this, new(-5, -4), new(-1, 0)), "a", "1");
                Pins.Add(new FixedOrientedPin("0", "The '0' input.", this, new(-5, 4), new(-1, 0)), "b", "0");
                Pins.Add(new FixedOrientedPin("c", "The controlling input.", this, new(0, -6), new(0, -1)), "c");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "output");

                DrawingVariants = Variant.Do(DrawMultiplexer);
            }
            private void DrawMultiplexer(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.Polygon(new[] {
                    new Vector2(-5, -8),
                    new Vector2(5, -4),
                    new Vector2(5, 4),
                    new Vector2(-5, 8)
                });

                drawing.Text("1", new Vector2(-4, -4), new Vector2(1, 0), new("small"));
                drawing.Text("0", new Vector2(-4, 4), new Vector2(1, 0), new("small"));
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(4, 6), new Vector2(1, 1));
            }
        }
    }
}
