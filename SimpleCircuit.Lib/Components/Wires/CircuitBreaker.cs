using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Wires
{
    [Drawable("CB", "A circuit breaker.", "Wires")]
    public class CircuitBreaker : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            var device = new Instance(name, options);
            switch (options?.Style ?? Options.Styles.ANSI)
            {
                case Options.Styles.AREI:
                    device.AddVariant(Options.Arei);
                    break;
                case Options.Styles.IEC:
                    device.AddVariant(Options.Iec);
                    break;
                default:
                    device.AddVariant(Options.Ansi);
                    break;
            }
            return device;
        }

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the circuit breaker.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "circuitbreaker";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("control", "The control pin.", this, new(0, -1.875), new(0, -1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("backside", "The backside control pin.", this, new(0, -1.875), new(0, 1)), "c2", "ctrl2");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "b", "n", "neg");

                PinUpdate = Variant.Do(UpdatePins);
                DrawingVariants = Variant.FirstOf(
                    Variant.If(Options.Arei).Then(DrawCircuitBreakerArei),
                    Variant.If(Options.Iec).Then(DrawCircuitBreakerIec),
                    Variant.Do(DrawRegular));
            }

            private void DrawRegular(SvgDrawing drawing)
            {
                // ANSI style circuit breaker
                drawing.ExtendPins(Pins, 2, "a", "b");

                drawing.OpenBezier(new Vector2[]
                {
                    new(-4, -2),
                    new(-2, -4.5),
                    new(2, -4.5),
                    new(4, -2)
                });

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, 3), new(0, 1));
            }

            private void DrawCircuitBreakerIec(SvgDrawing drawing)
            {
                // IEC style circuit breaker
                drawing.ExtendPins(Pins, 2, "a", "b");

                drawing.Line(new(-4, 0), new(4, -4));
                drawing.Cross(new(4, 0), 2);

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, 3), new(0, 1));
            }

            private void DrawCircuitBreakerArei(SvgDrawing drawing)
            {
                // AREI style circuit breaker
                drawing.ExtendPins(Pins, 2, "a", "b");

                drawing.Line(new(-6, 0), new(-4, 0), new("wire"));
                drawing.Line(new(-4, 0), new(4, -4));
                drawing.Polygon(new Vector2[]
                {
                    new(4, -4), new(3.25, -5.5),
                    new(1.25, -4.5), new(2, -3)
                }, new("dot"));

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, 3), new(0, 1));
            }

            private void UpdatePins()
            {
                if (HasVariant(Options.Arei) || HasVariant(Options.Iec))
                {
                    SetPinOffset(1, new(0, -2));
                    SetPinOffset(2, new(0, -2));
                }
                else
                {
                    SetPinOffset(1, new(0, -1.875));
                    SetPinOffset(2, new(0, -1.875));
                }
            }
        }
    }
}
