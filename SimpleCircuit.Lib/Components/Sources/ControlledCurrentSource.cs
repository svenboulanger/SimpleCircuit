using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A controlled current source.
    /// </summary>
    [Drawable("G", "A controlled current source.", "Sources", labelCount: 2)]
    [Drawable("F", "A controlled current source.", "Sources", labelCount: 2)]
    public class ControlledCurrentSource : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);

            /// <inheritdoc />
            public override string Type => "ccs";

            [Description("The margin for labels.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

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

                                _anchors[0] = new LabelAnchorPoint(new(0, -4 - LabelMargin), new(0, -1));
                                _anchors[1] = new LabelAnchorPoint(new(0, 4 + LabelMargin), new(0, 1));
                                break;

                            case 0:
                            default:
                                SetPinOffset(0, new(-6, 0));
                                SetPinOffset(1, new(6, 0));

                                _anchors[0] = new LabelAnchorPoint(new(0, -6 - LabelMargin), new(0, -1));
                                _anchors[1] = new LabelAnchorPoint(new(0, 6 + LabelMargin), new(0, 1));
                                break;
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                builder.ExtendPins(Pins, style);
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        DrawEuropeanSource(builder, style);
                        break;

                    case 0:
                    default:
                        DrawAmericanSource(builder, style);
                        break;
                }
            }

            /// <inheritdoc/>
            private void DrawAmericanSource(IGraphicsBuilder drawing, IStyle style)
            {
                // Diamond
                drawing.Polygon([
                    new(-6, 0),
                    new(0, 6),
                    new(6, 0),
                    new(0, -6)
                ], style);

                // The circle with the arrow
                drawing.Arrow(new(-3, 0), new(3, 0), style);
                _anchors.Draw(drawing, this, style);
            }
            private void DrawEuropeanSource(IGraphicsBuilder drawing, IStyle style)
            {
                drawing.Polygon([
                    new(-4, 0),
                    new(0, 4),
                    new(4, 0),
                    new(0, -4)
                ], style);
                drawing.Line(new(0, -4), new(0, 4), style);
                _anchors.Draw(drawing, this, style);
            }
        }
    }
}