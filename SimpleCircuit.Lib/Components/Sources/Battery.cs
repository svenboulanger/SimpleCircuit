using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A battery.
    /// </summary>
    [Drawable("BAT", "A battery.", "Sources", "cell cells", labelCount: 2)]
    public class Battery : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);

            private int _cells = 1;
            private double Length => _cells * 4 - 2;

            [Description("The number of cells.")]
            [Alias("c")]
            public int Cells
            {
                get => _cells;
                set
                {
                    _cells = value;
                    if (_cells < 1)
                        _cells = 1;
                }
            }

            /// <inheritdoc />
            public override string Type => "battery";

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
                Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-1, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(1, 0), new(1, 0)), "p", "pos", "a");
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
                        double offset = Length / 2;
                        SetPinOffset(0, new(-offset, 0));
                        SetPinOffset(1, new(offset, 0));
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                double m = style.LineThickness * 0.5 + LabelMargin;
                _anchors[0] = new LabelAnchorPoint(new(0, -6 - m), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 6 + m), new(0, 1));
                var negStyle = style.AsLineThickness(0.75);

                // Wires
                double offset = Length / 2;
                builder.ExtendPins(Pins, style);

                // The cells
                double x = -offset;
                for (int i = 0; i < _cells; i++)
                {
                    builder.Line(new(x, -2), new(x, 2), negStyle);
                    x += 2.0;
                    builder.Line(new(x, -6), new(x, 6), style);
                    x += 2.0;
                }

                // Add a little plus and minus next to the terminals!
                builder.Signs(new(offset + 3, 3), new(-offset - 3, 3), style, upright: true);

                // Depending on the orientation, let's anchor the text differently
                _anchors.Draw(builder, this, style);
            }
        }
    }
}