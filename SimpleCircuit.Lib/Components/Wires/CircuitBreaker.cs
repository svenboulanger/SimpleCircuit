using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Wires
{
    [Drawable("CB", "A circuit breaker.", "Wires")]
    public class CircuitBreaker : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new(2);

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.AREI | Standards.European | Standards.American;

            /// <inheritdoc />
            public override string Type => "circuitbreaker";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("control", "The control pin.", this, new(0, -1.875), new(0, -1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("backside", "The backside control pin.", this, new(0, -1.875), new(0, 1)), "c2", "ctrl2");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "b", "n", "neg");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                if (Variants.Contains(Options.Arei) || Variants.Contains(Options.European))
                {
                    SetPinOffset(1, new(0, -2));
                    SetPinOffset(2, new(0, -2));
                }
                else
                {
                    SetPinOffset(1, new(0, -1.875));
                    SetPinOffset(2, new(0, -1.875));
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.Arei, Options.European, Options.American))
                {
                    case 0: DrawCircuitBreakerArei(drawing); break;
                    case 1: DrawCircuitBreakerIec(drawing); break;
                    case 2:
                    default: DrawRegular(drawing); break;
                }
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

                drawing.Label(Labels, 0, new(0, 2), new(0, 1));
                drawing.Label(Labels, 1, new(0, -5.5), new(0, -1)); 
            }

            private void DrawCircuitBreakerIec(SvgDrawing drawing)
            {
                // IEC style circuit breaker
                drawing.ExtendPins(Pins, 2, "a", "b");

                drawing.Line(new(-4, 0), new(4, -4));
                drawing.Cross(new(4, 0), 2);

                drawing.Label(Labels, 0, new(0, 2), new(0, 1));
                drawing.Label(Labels, 1, new(0, -4), new(0, -1));
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
                }, new("marker"));

                drawing.Label(Labels, 0, new(0, 2), new(0, 1));
                drawing.Label(Labels, 1, new(0, -5.5), new(0, -1));
            }
        }
    }
}
