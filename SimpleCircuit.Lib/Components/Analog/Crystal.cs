using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    [Drawable("XTAL", "A crystal.", "Analog")]
    public class Crystal : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "crystal";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4.5, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4.5, 0), new(1, 0)), "n", "neg", "b");
                DrawingVariants = Variant.Do(DrawCrystal);
            }

            private void DrawCrystal(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(-4.5, 0), new(-6, 0), new("wire"));
                if (Pins[1].Connections == 0)
                    drawing.Line(new(4.5, 0), new(6, 0), new("wire"));

                // The crystal
                drawing.Rectangle(5, 10, options: new("body"));
                drawing.Path(b => b.MoveTo(-4.5, -3.5).Line(0, 7).MoveTo(4.5, -3.5).Line(0, 7));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -6), new(0, -1));
            }
        }
    }
}
