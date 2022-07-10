using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A bus wire segment.
    /// </summary>
    [Drawable("BUS", "A bus or wire segment.", "Wires")]
    public class Bus : DrawableFactory
    {
        private const string _straight = "straight";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The number of crossings. Can be used to indicate a bus.")]
            public int Crossings { get; set; } = 1;

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "bus";

            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("input", "The input.", this, new(0, 0), new(-1, 0)), "i", "a", "in", "input");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(0, 0), new(1, 0)), "o", "b", "out", "output");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, Crossings + 2);

                bool straight = Variants.Contains(_straight);
                if (Crossings > 0)
                {
                    drawing.Path(b =>
                    {
                        for (int i = 0; i < Crossings; i++)
                        {
                            double x = i * 2 - Crossings + 1;
                            if (straight)
                                b.MoveTo(x, 3).Line(0, -6);
                            else
                                b.MoveTo(x - 1.5, 3).Line(3, -6);
                        }
                    });
                }

                // The label
                drawing.Text(Labels[0], new(0, -4), new(0, -1));
            }
        }
    }
}