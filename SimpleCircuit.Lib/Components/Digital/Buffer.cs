using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Digital;

/// <summary>
/// An invertor.
/// </summary>
[Drawable("BUF", "A buffer.", "Digital", labelCount: 3)]
[Drawable("INV", "An invertor.", "Digital", labelCount: 3)]
[Drawable("NOT", "An invertor.", "Digital", labelCount: 3)]
public class Buffer : DrawableFactory
{
    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name, !key.Equals("BUF"));

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(2);
        private readonly bool _invertOutput;

        /// <inheritdoc />
        public override string Type => "buffer";

        /// <inheritdoc />
        [Description("The margin for labels to the edge.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="invertOutput">If <c>true</c>, the output is inverted.</param>
        public Instance(string name, bool invertOutput)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("input", "The input pin.", this, new(-6, 0), new(-1, 0)), "in", "input");
            Pins.Add(new FixedOrientedPin("positivepower", "The positive power pin.", this, new(0, -3), new(0, -1)), "vpos", "vp");
            Pins.Add(new FixedOrientedPin("negativepower", "The negative power pin.", this, new(0, 3), new(0, 1)), "vneg", "vn");
            Pins.Add(new FixedOrientedPin("output", "The output pin.", this, new(6, 0), new(1, 0)), "out", "output");
            _invertOutput = invertOutput;
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
                    switch (Variants.Select(Options.European, Options.American))
                    {
                        case 0:
                            SetPinOffset(0, new(-5, 0));
                            SetPinOffset(1, new(0, -5));
                            SetPinOffset(2, new(0, 5));
                            SetPinOffset(3, new(_invertOutput ? 8 : 5, 0));
                            break;

                        case 1:
                        default:
                            SetPinOffset(0, new(-6, 0));
                            SetPinOffset(1, new(0, -3));
                            SetPinOffset(2, new(0, 3));
                            SetPinOffset(3, new(_invertOutput ? 9 : 6, 0));
                            break;
                    }

                    var style = context.Style.ModifyDashedDotted(this);
                    double m = style.LineThickness * 0.5 + LabelMargin;
                    switch (Variants.Select(Options.European, Options.American))
                    {

                        case 0:
                            _anchors[0] = new LabelAnchorPoint(new(0, -5 - m), new(0, -1));
                            _anchors[1] = new LabelAnchorPoint(new(0, 5 + m), new(0, 1));
                            break;

                        default:
                            _anchors[0] = new LabelAnchorPoint(new(0, -6 - m), new(0, -1));
                            _anchors[1] = new LabelAnchorPoint(new(0, 6 + m), new(0, 1));
                            break;
                    };
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            switch (Variants.Select(Options.European, Options.American))
            {
                case 0: DrawBufferIEC(builder, style); break;
                case 1:
                default: DrawBuffer(builder, style); break;
            }
        }
        private void DrawBuffer(IGraphicsBuilder builder, IStyle style)
        {
            builder.ExtendPins(Pins, style, 2, "in", "out");
            builder.Polygon([
                new(-6, 6),
                new(6, 0),
                new(-6, -6)
            ], style);
            if (_invertOutput)
                builder.Circle(new(7.5, 0), 1.5, style);
            _anchors.Draw(builder, this, style);
        }

        private void DrawBufferIEC(IGraphicsBuilder builder, IStyle style)
        {
            builder.ExtendPins(Pins, style, 2, "in", "out");

            builder.Rectangle(-5, -5, 10, 10, style, new());
            if (_invertOutput)
                builder.Circle(new(6.5, 0), 1.5, style);

            var span = builder.TextFormatter.Format("1", style);
            builder.Text(span, -span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.Transformed);

            _anchors.Draw(builder, this, style);
        }
    }
}