using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.General
{
    /// <summary>
    /// A simple piece of text.
    /// </summary>
    [SimpleKey("TEXT", "A simple text element.", Category = "General")]
    public class Text : OrientedDrawable, ILabeled
    {
        /// <inheritdoc />
        public string Label { get; set; } = "Text";

        public Text(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("X", "The anchor point.", this, new(), new(1, 0)), "x");
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Text(Label, Location, new(1, 0));
        }

        public override string ToString() => $"Text {Name}";
    }
}
