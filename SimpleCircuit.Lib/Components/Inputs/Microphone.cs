using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Inputs
{
    /// <summary>
    /// A microphone.
    /// </summary>
    [Drawable("MIC", "A microphone.", "Inputs")]
    public class Microphone : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "mic";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name"></param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(0, -4), new(0, -1)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(0, 4), new(0, 1)), "n", "neg", "b");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.Circle(new(), 4);
                drawing.Line(new(4, -4), new(4, 4), new("plane"));

                Labels.SetDefaultPin(0, location: new(-6, 0), expand: new(-1, 0));
                Labels.Draw(drawing);
            }
        }
    }
}
