using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An invertor.
    /// </summary>
    [Drawable(new[] { "INV", "NOT" }, "An invertor.", new[] { "Digital" })]
    public class Invertor : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the inverter.")]
            public string Label { get; set; }
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("input", "The input pin.", this, new(-6, 0), new(-1, 0)), "in", "input");
                Pins.Add(new FixedOrientedPin("output", "The output pin.", this, new(9, 0), new(1, 0)), "out", "output");

                DrawingVariants = Variant.Do(DrawInverter);
            }

            /// <inheritdoc/>
            private void DrawInverter(SvgDrawing drawing)
            {
                drawing.Polygon(new[]
                {
                    new Vector2(-6, 6), new Vector2(6, 0), new Vector2(-6, -6)
                });
                drawing.Circle(new Vector2(7.5, 0), 1.5);

                if (!string.IsNullOrEmpty(Label))
                    drawing.Text(Label, new Vector2(-2.5, 0), new Vector2());
            }
        }
    }
}