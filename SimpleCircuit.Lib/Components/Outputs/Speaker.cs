using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A speaker.
    /// </summary>
    [Drawable("SPEAKER", "A speaker.", "Outputs", "sound music audio")]
    public class Speaker : DrawableFactory
    {
        private const string _off = "off";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;

            /// <inheritdoc />
            public override string Type => "speaker";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(0, -4), new(0, -1)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(0, 4), new(0, 1)), "n", "neg", "b");
                _anchors = new(
                new LabelAnchorPoint(new(8, 10), new(1, 1), Appearance));
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, this);
                builder.Rectangle(-2, -4, 4, 8);
                builder.Polygon([
                    new(2, -4),
                    new(6, -9),
                    new(6, 9),
                    new(2, 4)
                ]);

                if (!Variants.Contains(_off))
                    DrawOn(builder);

                _anchors.Draw(builder, this);
            }
            private void DrawOn(IGraphicsBuilder builder)
            {
                DrawSoundWave(builder, 8, 0, 3);
                DrawSoundWave(builder, 10, 0, 5);
                DrawSoundWave(builder, 12, 0, 7);
            }
            private void DrawSoundWave(IGraphicsBuilder builder, double x, double y, double s)
            {
                builder.Path(b => b
                    .MoveTo(new(x, y - s))
                    .CurveTo(new(x + s * 0.5, y - s * 0.5), new(x + s * 0.5, y + s * 0.5), new(x, y + s)));
            }
        }
    }
}