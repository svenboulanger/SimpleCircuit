using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Outputs;

/// <summary>
/// A speaker.
/// </summary>
[Drawable("SPEAKER", "A speaker.", "Outputs", "sound music audio")]
public class Speaker : DrawableFactory
{
    private const string _off = "off";

    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(1);

        /// <inheritdoc />
        public override string Type => "speaker";

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
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(0, -4), new(0, -1)), "p", "pos", "a");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(0, 4), new(0, 1)), "n", "neg", "b");
            _anchors = new(
            new LabelAnchorPoint(new(8, 10), new(1, 1)));
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            double m = style.LineThickness * 0.5 + LabelMargin;
            _anchors[0] = new LabelAnchorPoint(new(6 + m, 0), new(1, 0));

            builder.ExtendPins(Pins, style);
            builder.Rectangle(-2, -4, 4, 8, style);
            builder.Polygon([
                new(2, -4),
                new(6, -9),
                new(6, 9),
                new(2, 4)
            ], style);

            if (!Variants.Contains(_off))
            {
                DrawOn(builder, style);
                _anchors[0] = new LabelAnchorPoint(new(15.5 + m, 0), new(1, 0));
            }

            _anchors.Draw(builder, this, style);
        }
        private void DrawOn(IGraphicsBuilder builder, IStyle style)
        {
            DrawSoundWave(builder, 8, 0, 3, style);
            DrawSoundWave(builder, 10, 0, 5, style);
            DrawSoundWave(builder, 12, 0, 7, style);
        }
        private void DrawSoundWave(IGraphicsBuilder builder, double x, double y, double s, IStyle style)
        {
            builder.Path(b => b
                .MoveTo(new(x, y - s))
                .CurveTo(new(x + s * 0.5, y - s * 0.5), new(x + s * 0.5, y + s * 0.5), new(x, y + s)), style.AsStroke());
        }
    }
}