using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// Or gate.
    /// </summary>
    [Drawable("OR", "An OR gate.", "Digital")]
    public class Or : DrawableFactory
    {
        private const string _iec = "iec";

        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            var device = new Instance(name, options);
            if (options.IEC)
                device.AddVariant(_iec);
            return device;
        }

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public override string Type => "or";

            /// <inheritdoc />
            public string Label { get; set; }

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("a", "The first input.", this, new(-4, -2.5), new(-1, 0)), "a");
                Pins.Add(new FixedOrientedPin("b", "The second input.", this, new(-4, 2.5), new(-1, 0)), "b");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(6, 0), new(1, 0)), "o", "out", "output");

                PinUpdate = Variant.Map(_iec, UpdatePins);
                DrawingVariants = Variant.FirstOf(
                    Variant.If(_iec).Then(DrawOrIEC),
                    Variant.Do(DrawOr));
            }
            private void DrawOr(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.ClosedBezier(new[]
                {
                    new Vector2(-5, 5),
                    new Vector2(-5, 5), new Vector2(-4, 5), new Vector2(-4, 5),
                    new Vector2(1, 5), new Vector2(4, 3), new Vector2(6, 0),
                    new Vector2(4, -3), new Vector2(1, -5), new Vector2(-4, -5),
                    new Vector2(-4, -5), new Vector2(-3, -5), new Vector2(-5, -5),
                    new Vector2(-3, -2), new Vector2(-3, 2), new Vector2(-5, 5)
                });

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -6), new(0, -1));
            }

            private void DrawOrIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                drawing.Rectangle(8, 10, new());
                drawing.Text("&#8805;1", new(), new());

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -6), new(0, -1));
            }

            private void UpdatePins(bool iec)
            {
                if (iec)
                {
                    SetPinOffset(0, new(-4, -2.5));
                    SetPinOffset(1, new(-4, 2.5));
                    SetPinOffset(2, new(4, 0));
                }
                else
                {
                    SetPinOffset(0, new(-4, -2.5));
                    SetPinOffset(1, new(-4, 2.5));
                    SetPinOffset(2, new(6, 0));
                }
            }
        }
    }
}
