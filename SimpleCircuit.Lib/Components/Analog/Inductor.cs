using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An inductor.
    /// </summary>
    [Drawable("L", "An inductor.", "Analog")]
    public class Inductor : DrawableFactory
    {
        private const string _dot = "dot";
        private const string _programmable = "programmable";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the inductor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "inductor";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // Inductor
                drawing.Path(b => b
                    .MoveTo(-6, 0)
                    .CurveTo(new(-6, -4), new(-2, -4), new(-2, 0))
                    .SmoothTo(new(-4, 4), new(-4, 0))
                    .SmoothTo(new(1, -4), new(1, 0))
                    .SmoothTo(new(-1, 4), new(-1, 0))
                    .SmoothTo(new(4, -4), new(4, 0))
                    .SmoothTo(new(2, 4), new(2, 0))
                    .SmoothTo(new(6, -4), new(6, 0)));

                if (Variants.Contains(_dot))
                    drawing.Dot(new(-8, 3.5));
                if (Variants.Contains(_programmable))
                    drawing.Arrow(new(-5, 5), new(6, -7));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -5), new Vector2(0, -1));
            }
        }
    }
}