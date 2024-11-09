using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Wires
{
    [Drawable("CB", "A circuit breaker.", "Wires", "automatic arei")]
    public class CircuitBreaker : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            /// <inheritdoc />
            public Labels Labels { get; } = new();

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
            protected override void Draw(IGraphicsBuilder builder)
            {
                switch (Variants.Select(Options.Arei, Options.European, Options.American))
                {
                    case 0: DrawCircuitBreakerArei(builder); break;
                    case 1: DrawCircuitBreakerIec(builder); break;
                    case 2:
                    default: DrawRegular(builder); break;
                }
            }
            private void DrawRegular(IGraphicsBuilder builder)
            {
                // ANSI style circuit breaker
                builder.ExtendPins(Pins, 2, "a", "b");

                builder.Path(b => b
                    .MoveTo(new(-4, -2))
                    .CurveTo(new(-2, -4.5), new(2, -4.5), new(4, -2)));

                _anchors[0] = new LabelAnchorPoint(new(0, -5.5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 2), new(0, 1));
                _anchors.Draw(builder, this);
            }

            private void DrawCircuitBreakerIec(IGraphicsBuilder builder)
            {
                // IEC style circuit breaker
                builder.ExtendPins(Pins, 2, "a", "b");

                builder.Line(new(-4, 0), new(4, -4));
                builder.Cross(new(4, 0), 2);

                _anchors[0] = new LabelAnchorPoint(new(0, -4), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 2), new(0, 1));
                _anchors.Draw(builder, this);
            }

            private void DrawCircuitBreakerArei(IGraphicsBuilder builder)
            {
                // AREI style circuit breaker
                builder.ExtendPins(Pins, 2, "a", "b");

                builder.Line(new(-6, 0), new(-4, 0), new("wire"));
                builder.Line(new(-4, 0), new(4, -4));

                builder.RequiredCSS.Add(".marker { fill: black; }");
                builder.Polygon(new Vector2[]
                {
                    new(4, -4), new(3.25, -5.5),
                    new(1.25, -4.5), new(2, -3)
                }, new("marker"));

                _anchors[0] = new LabelAnchorPoint(new(0, -6.5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 1), new(0, 1));
                _anchors.Draw(builder, this);
            }
        }
    }
}
