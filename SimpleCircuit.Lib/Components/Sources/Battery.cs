using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A battery.
    /// </summary>
    [Drawable("BAT", "A battery.", "Sources", "cell cells")]
    public class Battery : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;

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
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-1, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(1, 0), new(1, 0)), "p", "pos", "a");
                _anchors = new(
                    new LabelAnchorPoint(new(0, -8), new(0, -1)),
                    new LabelAnchorPoint(new(0, 8), new(0, 1)));
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