using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    [Drawable("XTAL", "A crystal.", "Analog")]
    public class Crystal : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly static CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -6), new(0, -1)),
                new LabelAnchorPoint(new(0, 6), new(0, 1)));

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
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, this);

                // The crystal
                var options = Appearance.CreatePathOptions(this);
                builder.Rectangle(-2.5, -5, 5, 10, options: options);
                builder.Path(b => b.MoveTo(new(-4.5, -3.5)).Line(new(0, 7)).MoveTo(new(4.5, -3.5)).Line(new(0, 7)), options);

                _anchors.Draw(builder, Labels);
            }
        }
    }
}
