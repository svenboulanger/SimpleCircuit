using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An invertor.
    /// </summary>
    [Drawable("BUF", "A buffer.", "Digital")]
    public class Buffer : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            /// <inheritdoc />
            public override string Type => "buffer";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.American | Standards.European;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("input", "The input pin.", this, new(-6, 0), new(-1, 0)), "in", "input");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power pin.", this, new(0, -3), new(0, -1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power pin.", this, new(0, 3), new(0, 1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("output", "The output pin.", this, new(6, 0), new(1, 0)), "out", "output");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                if (Variants.Contains(Options.European))
                {
                    SetPinOffset(0, new(-5, 0));
                    SetPinOffset(1, new(0, -5));
                    SetPinOffset(2, new(0, 5));
                    SetPinOffset(3, new(5, 0));
                }
                else
                {
                    SetPinOffset(0, new(-6, 0));
                    SetPinOffset(1, new(0, -3));
                    SetPinOffset(2, new(0, 3));
                    SetPinOffset(3, new(6, 0));
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.European, Options.American))
                {
                    case 0: DrawBufferIEC(drawing); break;
                    case 1:
                    default: DrawBuffer(drawing); break;
                }
            }
            private void DrawBuffer(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "in", "out");
                drawing.Polygon(new Vector2[]
                {
                    new(-6, 6), new(6, 0), new(-6, -6)
                });

                drawing.Text(Labels[0], new(0, -4), new(1, -1));
            }

            private void DrawBufferIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "in", "out");

                drawing.Rectangle(-5, -5, 10, 10, new());
                drawing.Text("1", new(), new());

                drawing.Text(Labels[0], new(0, -6), new(0, -1));
            }
        }
    }
}