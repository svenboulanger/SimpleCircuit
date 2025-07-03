using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A controlled voltage source.
    /// </summary>
    [Drawable("E", "A controlled voltage source.", "Sources", labelCount: 2)]
    [Drawable("H", "A controlled voltage source.", "Sources", labelCount: 2)]
    public class ControlledVoltageSource : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);

            /// <inheritdoc />
            public override string Type => "cvs";

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
                        var style = context.Style.ModifyDashedDotted(this);
                        double m = style.LineThickness * 0.5 + LabelMargin;
                        switch (Variants.Select(Options.American, Options.European))
                        {
                            case 1:
                                SetPinOffset(0, new(-4, 0));
                                SetPinOffset(1, new(4, 0));

                                _anchors[0] = new LabelAnchorPoint(new(0, -4 - m), new(0, -1));
                                _anchors[1] = new LabelAnchorPoint(new(0, 4 + m), new(0, 1));
                                break;

                            case 0:
                            default:
                                SetPinOffset(0, new(-6, 0));
                                SetPinOffset(1, new(6, 0));

                                _anchors[0] = new LabelAnchorPoint(new(0, -6 - m), new(0, -1));
                                _anchors[1] = new LabelAnchorPoint(new(0, 6 + m), new(0, 1));
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

            /// <inheritdoc />
            private void DrawAmericanSource(IGraphicsBuilder builder, IStyle style)
            {
                // Diamond
                builder.Polygon([
                    new(-6, 0),
                    new(0, 6),
                    new(6, 0),
                    new(0, -6)
                ], style);

                // Plus and minus
                builder.Signs(new(3, 0), new(-3, 0), style, vertical: true);
                _anchors.Draw(builder, this, style);
            }
            private void DrawEuropeanSource(IGraphicsBuilder builder, IStyle style)
            {
                builder.Polygon(
                [
                    new(-4, 0), new(0, 4), new(4, 0), new(0, -4)
                ], style);
                builder.Line(new(-4, 0), new(4, 0), style);
                _anchors.Draw(builder, this, style);
            }
        }
    }
}