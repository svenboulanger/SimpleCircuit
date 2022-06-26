using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A resistor.
    /// </summary>
    [Drawable("R", "A resistor, potentially programmable.", "Analog")]
    public class ResistorFactory : DrawableFactory
    {
        private const string _programmable = "programmable";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {

            /// <inheritdoc/>
            [Description("The label next to the resistor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "resistor";

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("p", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("ctrl", "The controlling pin.", this, new(0, 8), new(0, 1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("n", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "neg", "b");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                switch (Variants.Select(Options.Ansi, Options.Iec))
                {
                    case 1:
                        DrawIECResistor(drawing);
                        break;
                    case 0:
                    default:
                        DrawANSIResistor(drawing);
                        break;
                }
                if (Variants.Contains(_programmable))
                    drawing.Arrow(new(-5, 5), new(6, -7));
            }
            private void DrawANSIResistor(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                // The resistor
                drawing.Polyline(new Vector2[]
                {
                    new(-6, 0), new(-5, -4),
                    new(-3, 4), new(-1, -4),
                    new(1, 4), new(3, -4),
                    new(5, 4), new(6, 0)
                });

                // Controlled resistor
                if (Pins[1].Connections > 0)
                    CommonGraphical.Arrow(drawing, new Vector2(0, 8), new Vector2(0, 4));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
            }
            private void DrawIECResistor(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                // The rectangle
                drawing.Rectangle(12, 6);

                // The label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -7), new(0, -1));
            }
        }
    }
}