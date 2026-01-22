using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Wires;

/// <summary>
/// A fuse.
/// </summary>
[Drawable("FUSE", "A fuse.", "Wires", labelCount: 2)]
public class Fuse : DrawableFactory
{
    private const string _alt = "alt";

    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(2);

        /// <inheritdoc />
        public override string Type => "fuse";

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
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "a", "p", "pos");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "b", "n", "neg");
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            double m = style.LineThickness * 0.5 + LabelMargin;
            _anchors[0] = new LabelAnchorPoint(new(0, -3 - m), new(0, -1));
            _anchors[1] = new LabelAnchorPoint(new(0, 3 + m), new(0, 1));
            switch (Variants.Select(Options.European, Options.American))
            {
                case 0: DrawIEC(builder, style); break;
                case 1:
                default:
                    if (Variants.Contains(_alt))
                        DrawANSIalt(builder, style);
                    else
                        DrawANSI(builder, style);
                    break;
            }
        }
        private void DrawIEC(IGraphicsBuilder builder, IStyle style)
        {
            builder.ExtendPins(Pins, style);

            builder.Rectangle(-6, -3, 12, 6, style);
            builder.Path(b => b.MoveTo(new(-3.5, -3)).Line(new(0, 6)).MoveTo(new(3.5, -3)).Line(new(0, 6)), style);

            _anchors.Draw(builder, this, style);
        }
        private void DrawANSI(IGraphicsBuilder builder, IStyle style)
        {
            builder.ExtendPins(Pins, style);

            builder.Rectangle(-6, -3, 12, 6, style);
            builder.Line(new(-6, 0), new(6, 0), style);

            _anchors.Draw(builder, this, style);
        }
        private void DrawANSIalt(IGraphicsBuilder builder, IStyle style)
        {
            builder.ExtendPins(Pins, style);
            builder.Path(b => b
                .MoveTo(new(-6, 0))
                .CurveTo(new(-6, -1.65685424949), new(-4.65685424949, -3), new(-3, -3))
                .CurveTo(new(-1.34314575051, -3), new(0, -1.65685424949), new())
                .CurveTo(new(0, 1.65685424949), new(1.34314575051, 3), new(3, 3))
                .CurveTo(new(4.65685424949, 3), new(6, 1.65685424949), new(6, 0)), style.AsStroke());

            _anchors.Draw(builder, this, style);
        }
    }
}