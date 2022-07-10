using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A speaker.
    /// </summary>
    [Drawable("SPEAKER", "A speaker.", "Outputs")]
    public class Speaker : DrawableFactory
    {
        private const string _off = "off";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new();

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
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.Rectangle(4, 8, new());
                drawing.Polygon(new Vector2[]
                {
                    new(2, -4), new(6, -9),
                    new(6, 9), new(2, 4)
                });

                if (!Variants.Contains(_off))
                    DrawOn(drawing);

                drawing.Text(Labels[0], new(8, 10), new(1, 1));
            }
            private void DrawOn(SvgDrawing drawing)
            {
                DrawSoundWave(drawing, 8, 0, 3);
                DrawSoundWave(drawing, 10, 0, 5);
                DrawSoundWave(drawing, 12, 0, 7);
            }
            private void DrawSoundWave(SvgDrawing drawing, double x, double y, double s)
            {
                drawing.OpenBezier(new Vector2[]
                {
                    new(x, y - s),
                    new(x + s * 0.5, y - s * 0.5), new(x + s * 0.5, y + s * 0.5), new(x, y + s)
                });
            }
        }
    }
}