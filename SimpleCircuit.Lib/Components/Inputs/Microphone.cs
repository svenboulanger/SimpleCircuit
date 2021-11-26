using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Inputs
{
    [SimpleKey("MIC", "A microphone.", Category = "Inputs")]
    public class Microphone : ScaledOrientedDrawable, ILabeled
    {
        [Description("Adds a label next to the microphone.")]
        public string Label { get; set; }

        /// <summary>
        /// Creates a new microphone.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options.</param>
        public Microphone(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(0, -4), new(0, -1)), "p", "pos", "a");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(0, 4), new(0, 1)), "n", "neg", "b");

            DrawingVariants = Variant.Do(DrawMic);
        }

        private void DrawMic(SvgDrawing drawing)
        {
            drawing.Circle(new(), 4);
            drawing.Line(new(4, -4), new(4, 4), new("plane"));

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new(-6, 0), new(-1, 0));
        }

        public override string ToString() => $"Microphone {Name}";
    }
}
