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
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The power plane name.")]
            public string Label { get; set; } = "VDD";

            /// <inheritdoc />
            public override string Type => "power";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, 1)), "x", "p", "a");
            }

            /// <inheritdoc />
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