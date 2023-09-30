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
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "connector";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name"></param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(0, 1.5), new(0, 1)), "p", "a", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "n", "b", "neg");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPin(Pins["p"], 4);
                drawing.ExtendPin(Pins["n"]);
                drawing.Circle(new(), 1.5);
                drawing.Circle(new(), 4);
                drawing.Circle(new(4, 0), 1, new("marker"));

                Labels.SetDefaultPin(-1, location: new(-6, 0), expand: new(-1, 0));
                Labels.Draw(drawing);
            }
        }
    }
}