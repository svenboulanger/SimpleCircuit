using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    [Drawable("XTAL", "A crystal.", "Analog")]
    public class Crystal : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new(2);

            /// <inheritdoc />
            public override string Type => "crystal";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4.5, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4.5, 0), new(1, 0)), "n", "neg", "b");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // The crystal
                drawing.Rectangle(-2.5, -5, 5, 10, options: new("body"));
                drawing.Path(b => b.MoveTo(-4.5, -3.5).Line(0, 7).MoveTo(4.5, -3.5).Line(0, 7));

                // Label
                Labels.Draw(drawing, 0, new(0, -6), new(0, -1));
                Labels.Draw(drawing, 1, new(0, 6), new(0, 1));
            }
        }
    }
}
