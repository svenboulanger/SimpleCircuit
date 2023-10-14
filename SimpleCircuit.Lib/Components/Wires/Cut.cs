using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Wires
{
    [Drawable("CUT", "A wire cut.", "Wires")]
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
            public double Gap { get; set; } = 2.0;

            [Description("The height of the gap edges. The default is 8.")]
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
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                double h = 0.5 * Height;
                double w = 0.5 * Gap;
                switch (Variants.Select(_straight, _none))
                {
                    case 0:
                        drawing.Line(new(-w, -h), new(-w, h));
                        drawing.Line(new(w, -h), new(w, h));
                        break;

                    case 1:
                        break;

                    default:
                        drawing.Line(new(-w - h * 0.25, -h), new(-w + h * 0.25, h));
                        drawing.Line(new(w - h * 0.25, -h), new(w + h * 0.25, h));
                        break;
                }

                _anchors.Draw(drawing, this);
            }
        }
    }
}
