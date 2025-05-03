using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A controlled current source.
    /// </summary>
    [Drawable("G", "A controlled current source.", "Sources")]
    [Drawable("F", "A controlled current source.", "Sources")]
    public class ControlledCurrentSource : DrawableFactory
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
            public override string Type => "ccs";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The current end point.", this, new(-6, 0), new(-1, 0)), "p", "b");
                Pins.Add(new FixedOrientedPin("negative", "The current starting point.", this, new(6, 0), new(1, 0)), "n", "a");
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

            /// <inheritdoc/>
            private void DrawAmericanSource(IGraphicsBuilder drawing)
            {
                // Diamond
                drawing.Polygon([
                    new(-6, 0),
                    new(0, 6),
                    new(6, 0),
                    new(0, -6)
                ]);

                // The circle with the arrow
                drawing.Arrow(new(-3, 0), new(3, 0), Appearance, this);

                _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1));
                _anchors.Draw(drawing, this);
            }
            private void DrawEuropeanSource(IGraphicsBuilder drawing)
            {
                drawing.Polygon([
                    new(-4, 0),
                    new(0, 4),
                    new(4, 0),
                    new(0, -4)
                ]);
                drawing.Line(new(0, -4), new(0, 4));

                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1));
                _anchors.Draw(drawing, this);
            }
        }
    }
}