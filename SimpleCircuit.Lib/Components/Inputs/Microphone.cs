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
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("Adds a label next to the microphone.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "mic";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(0, -4), new(0, -1)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(0, 4), new(0, 1)), "n", "neg", "b");
                DrawingVariants = Variant.Do(DrawMic);
            }
            private void DrawMic(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.Circle(new(), 4);
                drawing.Line(new(4, -4), new(4, 4), new("plane"));

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(-6, 0), new(-1, 0));
            }
        }
    }
}
