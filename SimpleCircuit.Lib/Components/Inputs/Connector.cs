using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;

namespace SimpleCircuit.Components.Inputs;

/// <summary>
/// A connector.
/// </summary>
[Drawable("CONN", "A connector or fastener.", "Inputs", "male female", labelCount: 2)]
public class Connector : DrawableFactory
{
    private const string _male = "male";
    private const string _female = "female";

    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private CustomLabelAnchorPoints _anchors;

        /// <inheritdoc />
        public override string Type => "connector";

        /// <summary>
        /// The distance from the label to the symbol.
        /// </summary>
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
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(-4, 0), new(-1, 0)), "n", "neg", "b");
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(2, 0), new(1, 0)), "p", "pos", "a");
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
                    _anchors = new(
                        new LabelAnchorPoint(new(0, -4 - m), new(0, -1)),
                        new LabelAnchorPoint(new(0, 4 + m), new(0, 1)));

                    switch (Variants.Select(Options.American))
                    {
                        case 0:
                            switch (Variants.Select(_male, _female))
                            {
                                case 0:
                                case 1:
                                    SetPinOffset(0, new());
                                    SetPinOffset(1, new());
                                    break;

                                default:
                                    SetPinOffset(0, new(-2, 0));
                                    SetPinOffset(1, new(2, 0));
                                    break;
                            }
                            break;

                        default:
                            SetPinOffset(0, new(-4, 0));
                            SetPinOffset(1, new(1.5, 0));
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
            switch (Variants.Select(Options.American))
            {
                case 0:
                    switch (Variants.Select(_male, _female))
                    {
                        case 0:
                            builder.Polyline([
                                new(-4, 4),
                                new(),
                                new(-4, -4)
                            ], style);
                            builder.ExtendPin(Pins["n"], style, 5);
                            break;

                        case 1:
                            builder.Polyline([
                                new(4, 4),
                                new(),
                                new(4, -4)
                            ], style);
                            builder.ExtendPin(Pins["n"], style, 5);
                            break;

                        default:
                            builder.Polyline([
                                new(-6, 4),
                                new(-2, 0),
                                new(-6, -4)
                            ], style);
                            
                            builder.Polyline([
                                new(-2, 4),
                                new(2, 0),
                                new(-2, -4)
                            ], style);

                            builder.ExtendPins(Pins, style,  5);
                            break;
                    }
                    break;

                default:
                    double s = Math.Sqrt(2) * 2;
                    builder.Path(b => b.MoveTo(new(s, -s)).ArcTo(4, 4, 0, true, false, new(s, s)), style.AsStroke());
                    builder.Circle(new(), 1.5, style);
                    builder.ExtendPin(Pins["n"], style);
                    builder.ExtendPin(Pins["p"], style, 4);
                    break;
            }

            _anchors.Draw(builder, this, style);
        }
    }
}