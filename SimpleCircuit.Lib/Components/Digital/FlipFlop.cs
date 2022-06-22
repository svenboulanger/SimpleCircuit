using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A flip-flop.
    /// </summary>
    [Drawable("FF", "A general flip-flop.", "Digital")]
    public class FlipFlop : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the transistor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "flipflop";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("data", "The data pin.", this, new(-9, -6), new(-1, 0)), "d", "data");
                Pins.Add(new FixedOrientedPin("clock", "The clock pin.", this, new(-9, 6), new(-1, 0)), "c", "clock");
                Pins.Add(new FixedOrientedPin("reset", "The reset pin.", this, new(0, 12), new(0, 1)), "r", "rst", "reset");
                Pins.Add(new FixedOrientedPin("set", "The set pin.", this, new(0, -12), new(0, -1)), "s", "set");
                Pins.Add(new FixedOrientedPin("nq", "The inverted output pin.", this, new(9, 6), new(1, 0)), "nq", "qn");
                Pins.Add(new FixedOrientedPin("q", "The output pin.", this, new(9, -6), new(1, 0)), "q");
                DrawingVariants = Variant.Do(DrawFlipFlop);
            }
            private void DrawFlipFlop(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "d", "c", "q");

                // Body
                drawing.Rectangle(18, 24, new());

                // Clock thingy
                drawing.Polyline(new[]
                {
                    new Vector2(-9, 4), new Vector2(-7, 6), new Vector2(-9, 8)
                });
                drawing.Text("D", new Vector2(-8, -6), new Vector2(1, 0));
                drawing.Text("C", new Vector2(-6, 6), new Vector2(1, 0));
                drawing.Text("Q", new Vector2(8, -6), new Vector2(-1, 0));

                if (Pins["nq"].Connections > 0)
                    drawing.Text("nQ", new Vector2(8, 6), new Vector2(-1, 0));
                if (Pins["s"].Connections > 0)
                    drawing.Text("set", new Vector2(0, -11.5), new Vector2(0, 1), new("small"));
                if (Pins["r"].Connections > 0)
                    drawing.Text("rst", new Vector2(0, 11.5), new Vector2(0, -1), new("small"));
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(10, 12), new Vector2(1, -1));
            }
        }
    }
}