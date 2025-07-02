using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

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
            private readonly CustomLabelAnchorPoints _anchors = new(2);

            /// <inheritdoc />
            public override string Type => "crystal";

            [Description("The margin for labels.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

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
                var style = builder.Style.ModifyDashedDotted(this);
                builder.ExtendPins(Pins, style);

                // The crystal
                builder.Rectangle(-2.5, -5, 5, 10, style);
                builder.Path(b => b.MoveTo(new(-4.5, -3.5)).Line(new(0, 7)).MoveTo(new(4.5, -3.5)).Line(new(0, 7)), style);

                double m = style.LineThickness * 0.5 + LabelMargin;
                _anchors[0] = new LabelAnchorPoint(new(0, -5 - m), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 5 + m), new(0, 1));
                _anchors.Draw(builder, this, style);
            }
        }
    }
}
