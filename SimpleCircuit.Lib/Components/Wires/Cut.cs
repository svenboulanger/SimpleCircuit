using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Wires
{
    [Drawable("CUT", "A wire cut.", "Wires", "break arei")]
    public class Cut : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private readonly static CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -4), new(0, -1)),
                new LabelAnchorPoint(new(0, 4), new(0, 1)));
            private const string _straight = "straight";
            private const string _none = "none";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            [Description("The gap between the wires. The default is 2.")]
            [Alias("g")]
            public double Gap { get; set; } = 2.0;

            [Description("The height of the gap edges. The default is 8.")]
            [Alias("h")]
            public double Height { get; set; } = 8.0;

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
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

                // Reset the pin locations
                SetPinOffset(0, new(-Gap * 0.5, 0));
                SetPinOffset(1, new(Gap * 0.5, 0));
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins);

                double h = 0.5 * Height;
                double w = 0.5 * Gap;
                switch (Variants.Select(_straight, _none))
                {
                    case 0:
                        builder.Line(new(-w, -h), new(-w, h));
                        builder.Line(new(w, -h), new(w, h));
                        break;

                    case 1:
                        break;

                    default:
                        builder.Line(new(-w - h * 0.25, -h), new(-w + h * 0.25, h));
                        builder.Line(new(w - h * 0.25, -h), new(w + h * 0.25, h));
                        break;
                }

                _anchors[0] = new LabelAnchorPoint(new(0, -h - 1), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, h + 1), new(0, 1));
                _anchors.Draw(builder, this);
            }
        }
    }
}
