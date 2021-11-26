using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A speaker.
    /// </summary>
    [SimpleKey("SPEAKER", "A speaker.", Category = "Outputs")]
    public class Speaker : ScaledOrientedDrawable, ILabeled
    {
        [Description("Adds a label next to the speaker.")]
        public string Label { get; set; }

        /// <summary>
        /// Creates a new speaker.
        /// </summary>
        /// <param name="name">The name of the speaker.</param>
        /// <param name="options">Options.</param>
        public Speaker(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(0, -4), new(0, -1)), "p", "pos", "a");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(0, 4), new(0, 1)), "n", "neg", "b");

            DrawingVariants = Variant.All(
                Variant.Do(DrawSpeaker),
                Variant.IfNot("off").Do(DrawOn));
        }

        private void DrawSpeaker(SvgDrawing drawing)
        {
            drawing.Polygon(new Vector2[]
            {
                new(-2, -4), new(2, -4),
                new(2, 4), new(-2, 4)
            });
            drawing.Polygon(new Vector2[]
            {
                new(2, -4), new(6, -9),
                new(6, 9), new(2, 4)
            });

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new(8, 10), new(1, 1));
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

        public override string ToString() => $"Speaker {Name}";
    }
}
