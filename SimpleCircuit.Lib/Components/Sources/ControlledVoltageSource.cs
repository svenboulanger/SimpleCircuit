using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A controlled voltage source.
    /// </summary>
    [Drawable("E", "A controlled voltage source.", "Sources")]
    [Drawable("H", "A controlled voltage source.", "Sources")]
    public class ControlledVoltageSource : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            /// <inheritdoc />
            public override string Type => "cvs";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-6, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(6, 0), new(1, 0)), "p", "pos", "a");
            }

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        switch (Variants.Select(Options.American, Options.European))
                        {
                            case 1:
                                SetPinOffset(0, new(-4, 0));
                                SetPinOffset(1, new(4, 0));
                                break;

                            case 0:
                            default:
                                SetPinOffset(0, new(-6, 0));
                                SetPinOffset(1, new(6, 0));
                                break;
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, this);
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        DrawEuropeanSource(builder);
                        break;

                    case 0:
                    default:
                        DrawAmericanSource(builder);
                        break;
                }
            }

            /// <inheritdoc />
            private void DrawAmericanSource(IGraphicsBuilder builder)
            {
                // Diamond
                builder.Polygon([
                    new(-6, 0),
                    new(0, 6),
                    new(6, 0),
                    new(0, -6)
                ]);

                // Plus and minus
                builder.Line(new(-3, -1), new(-3, 1), new("minus"));
                builder.Path(b => b
                    .MoveTo(new(3, -1))
                    .Line(new(0, 2))
                    .MoveTo(new(2, 0))
                    .Line(new(2, 0)),
                    new("plus"));

                _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1), Appearance);
                _anchors.Draw(builder, this);
            }
            private void DrawEuropeanSource(IGraphicsBuilder builder)
            {
                builder.Polygon(
                [
                    new(-4, 0), new(0, 4), new(4, 0), new(0, -4)
                ]);
                builder.Line(new(-4, 0), new(4, 0));

                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1), Appearance);
                _anchors.Draw(builder, this);
            }
        }
    }
}