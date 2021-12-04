using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A factory for power planes.
    /// </summary>
    [Drawable("POW", "A power plane.", "General")]
    public class PowerFactory : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The power plane name.")]
            public string Label { get; set; } = "VDD";
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, 1)), "x", "p", "a");
                DrawingVariants = Variant.Do(DrawPower);
            }
            private void DrawPower(SvgDrawing drawing)
            {
                // Wire
                drawing.Line(new Vector2(0, 0), new Vector2(0, -3), new("wire"));

                // Power
                drawing.Line(new Vector2(-5, -3), new Vector2(5, -3), new("plane"));
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
            }
        }
    }
}