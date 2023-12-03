using SimpleCircuit.Circuits.Contexts;
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

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private readonly static CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -8), new(0, -1)),
                new LabelAnchorPoint(new(0, 8), new(0, 1)));

            private int _cells = 1;
            private double Length => _cells * 4 - 2;

            /// <inheritdoc />
            public Labels Labels { get; } = new();

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
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                double offset = Length / 2;
                SetPinOffset(0, new(-offset, 0));
                SetPinOffset(1, new(offset, 0));
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.RequiredCSS.Add(".battery .neg { stroke-width: 0.75pt; }");

                // Wires
                double offset = Length / 2;
                drawing.ExtendPins(Pins);

                // The cells
                double x = -offset;
                for (int i = 0; i < _cells; i++)
                {
                    drawing.Line(new(x, -2), new(x, 2), new("neg"));
                    x += 2.0;
                    drawing.Line(new(x, -6), new(x, 6), new("pos"));
                    x += 2.0;
                }

                // Add a little plus and minus next to the terminals!
                drawing.Signs(new(offset + 2, 3), new(-offset - 2, 3), vertical: true);

                // Depending on the orientation, let's anchor the text differently
                _anchors.Draw(drawing, this);
            }
        }
    }
}