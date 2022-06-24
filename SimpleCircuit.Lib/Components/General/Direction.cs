using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.General
{
    /// <summary>
    /// A direction that is like a regular point, but can be oriented.
    /// This is useful for example when combined with subcircuits to give an orientation.
    /// </summary>
    [Drawable("DIR", "Directional point, useful for subcircuit definitions or indicating busses (using crossings).", "General")]
    public class Direction : DrawableFactory
    {
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name);

        private class Instance : OrientedDrawable, ILabeled
        {
            [Description("The label placed next to the wire.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "direction";

            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("input", "The input.", this, new(), new(-1, 0)), "i", "a", "in", "input");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(), new(1, 0)), "o", "b", "out", "output");
            }

            protected override void Draw(SvgDrawing drawing)
            {
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, 4), new(0, 1));
            }
        }
    }
}
