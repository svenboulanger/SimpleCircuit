using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Inputs
{
    /// <summary>
    /// A microphone.
    /// </summary>
    [Drawable("MIC", "A microphone.", "Inputs", labelCount: 2)]
    public class Microphone : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);

            /// <inheritdoc />
            public override string Type => "mic";

            [Description("The margin for labels.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

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
            protected override void Draw(IGraphicsBuilder builder)
            {
                _anchors[0] = new LabelAnchorPoint(new(-4 - LabelMargin, 0), new(-1, 0));
                _anchors[1] = new LabelAnchorPoint(new(4 + LabelMargin, 0), new(1, 0));

                var style = builder.Style.ModifyDashedDotted(this);
                builder.Circle(new(), 4, style);
                builder.Line(new(4, -4), new(4, 4), style.AsLineThickness(1.0));

                _anchors.Draw(builder, this, style);
                builder.ExtendPins(Pins, style);
            }
        }
    }
}
