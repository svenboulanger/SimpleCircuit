using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

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

        private class Instance : ScaledOrientedDrawable
        {
            private readonly static CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(-6, 0), new(-1, 0)),
                new LabelAnchorPoint(new(6, 0), new(1, 0)));

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
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.RequiredCSS.Add(".plane { stroke-width: 1pt; }");

                builder.ExtendPins(Pins);
                builder.Circle(new(), 4);
                builder.Line(new(4, -4), new(4, 4), new("plane"));
                _anchors.Draw(builder, this);
            }
        }
    }
}
