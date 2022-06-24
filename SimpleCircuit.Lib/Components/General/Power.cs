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

            /// <inheritdoc />
            public override string Type => "power";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, 1)), "x", "p", "a");
            }
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // Power
                drawing.Line(new Vector2(-5, 0), new Vector2(5, 0), new("plane"));
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -3), new Vector2(0, -1));
            }
        }
    }
}