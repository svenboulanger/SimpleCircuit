using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An invertor.
    /// </summary>
    [Drawable(new[] { "INV", "NOT" }, "An invertor.", new[] { "Digital" })]
    public class Invertor : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable, IBoxLabeled
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -4), new(1, -1)),
                new LabelAnchorPoint(new(0, 4), new(1, 1)));

            /// <inheritdoc />
            public override string Type => "invertor";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.American | Standards.European;

            [Description("The label margin to the edge.")]
            public double LabelMargin { get; set; }

            double IBoxLabeled.CornerRadius => 0.0;
            Vector2 IBoxLabeled.TopLeft => new(-5, -5);
            Vector2 IBoxLabeled.BottomRight => new(5, 5);

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
                Pins.Add(new FixedOrientedPin("output", "The output pin.", this, new(9, 0), new(1, 0)), "out", "output");
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
                    SetPinOffset(3, new(8, 0));
                }
                else
                {
                    SetPinOffset(0, new(-6, 0));
                    SetPinOffset(1, new(0, -3));
                    SetPinOffset(2, new(0, 3));
                    SetPinOffset(3, new(9, 0));
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.European, Options.American))
                {
                    case 0: DrawInverterIEC(drawing); break;
                    case 1:
                    default: DrawInverter(drawing); break;
                }
            }
            private void DrawInverter(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "in", "out");
                drawing.Polygon(new Vector2[]
                {
                    new(-6, 6), new(6, 0), new(-6, -6)
                });
                drawing.Circle(new(7.5, 0), 1.5);

                _anchors.Draw(drawing, this);
            }

            private void DrawInverterIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "in", "out");

                drawing.Rectangle(-5, -5, 10, 10);
                drawing.Circle(new(6.5, 0), 1.5);
                drawing.Text("1", new Vector2(), new Vector2());

                BoxLabelAnchorPoints.Default.Draw(drawing, this);
            }
        }
    }
}