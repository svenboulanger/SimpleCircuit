using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An impedance/admittance.
    /// </summary>
    [Drawable("Z", "An impedance.", "Analog")]
    [Drawable("Y", "An admittance.", "Analog")]
    public class Impedance : DrawableFactory
    {
        private const string _programmable = "programmable";

        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            var result = new Instance(key == "Y" ? "admittance" : "impedance", name, options);
            return result;
        }

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the element.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type { get; }

            public Instance(string type, string name, Options options)
                : base(name, options)
            {
                Type = type;
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "neg", "b");
            }
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // The rectangle
                CommonGraphical.Rectangle(drawing, 12, 6);

                if (Variants.Contains(_programmable))
                    drawing.Arrow(new(-5, 5), new(6, -7));

                // The label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -5), new(0, -1));
            }
        }
    }
}
