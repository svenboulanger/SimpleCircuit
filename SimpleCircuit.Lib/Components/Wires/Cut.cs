using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire cut.
    /// </summary>
    [Drawable("CUT", "A wire cut.", "Wires", "break arei", labelCount: 2)]
    public class Cut : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);
            private const string _straight = "straight";
            private const string _none = "none";

            [Description("The gap between the wires. The default is 2.")]
            [Alias("g")]
            public double Gap { get; set; } = 2.0;

            [Description("The height of the gap edges. The default is 8.")]
            [Alias("h")]
            public double Height { get; set; } = 8.0;

            [Description("The margin for the labels.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("a", "The first pin.", this, new(-2, 0), new(-1, 0)), "a");
                Pins.Add(new FixedOrientedPin("b", "The second pin.", this, new(2, 0), new(1, 0)), "b");
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
                        // Reset the pin locations
                        SetPinOffset(0, new(-Gap * 0.5, 0));
                        SetPinOffset(1, new(Gap * 0.5, 0));
                        
                        switch (Variants.Select(_straight, _none))
                        {
                            case 1:
                                _anchors[0] = new LabelAnchorPoint(new(0, -LabelMargin), new(0, -1));
                                _anchors[1] = new LabelAnchorPoint(new(0, LabelMargin), new(0, 1));
                                break;

                            default:
                                _anchors[0] = new LabelAnchorPoint(new(0, -0.5 * Height - LabelMargin), new(0, -1));
                                _anchors[1] = new LabelAnchorPoint(new(0, 0.5 * Height + LabelMargin), new(0, 1));
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

                double h = 0.5 * Height;
                double w = 0.5 * Gap;
                switch (Variants.Select(_straight, _none))
                {
                    case 0:
                        builder.Line(new(-w, -h), new(-w, h), style);
                        builder.Line(new(w, -h), new(w, h), style);
                        break;

                    case 1:
                        break;

                    default:
                        builder.Line(new(-w - h * 0.25, -h), new(-w + h * 0.25, h), style);
                        builder.Line(new(w - h * 0.25, -h), new(w + h * 0.25, h), style);
                        break;
                }
                _anchors.Draw(builder, this, style);
            }
        }
    }
}
