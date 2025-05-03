using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
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

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(new LabelAnchorPoint(new(-6, 0), new(-1, 0)));

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
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPin(Pins["p"], Appearance, this, 4);
                builder.ExtendPin(Pins["n"], Appearance, this);
                builder.Circle(new(), 1.5);
                builder.Circle(new(), 4);
                builder.Circle(new(4, 0), 1, new("marker"));

                _anchors.Draw(builder, this);
            }
        }
    }
}