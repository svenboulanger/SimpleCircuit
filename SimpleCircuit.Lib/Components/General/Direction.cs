using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.General
{
    /// <summary>
    /// A direction that is like a regular point, but can be oriented.
    /// This is useful for example when combined with subcircuits to give an orientation.
    /// </summary>
    [Drawable("DIR", "Directional point, useful for defining subcircuit definition ports.", "General")]
    public class Direction : DrawableFactory
    {
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : OrientedDrawable, ILabeled
        {
            [Description("The label placed next to the wire.")]
            public string Label { get; set; }

            [Description("The label placed on the other side of the wire.")]
            public string Label2 { get; set; }

            /// <inheritdoc />
            public override string Type => "direction";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("input", "The input.", this, new(), new(-1, 0)), "i", "a", "in", "input");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(), new(1, 0)), "o", "b", "out", "output");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.Text(Label, new(0, 3), new(0, 1));
                drawing.Text(Label2, new(0, -3), new(0, -1));
            }
        }
    }
}
