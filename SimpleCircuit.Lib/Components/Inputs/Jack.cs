using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Inputs;

/// <summary>
/// A jack.
/// </summary>
[Drawable("JACK", "A (phone) jack.", "Inputs")]
public class Jack : DrawableFactory
{
    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(1);

        /// <inheritdoc />
        public override string Type => "connector";

        /// <summary>
        /// The distance from the label to the symbol.
        /// </summary>
        [Description("The margin for labels.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name"></param>
        public Instance(string name)
            : base(name)
        {
            AddPin(new FixedOrientedPin("positive", "The positive pin.", this, new(0, 1.5), new(0, 1)), "p", "a", "pos");
            AddPin(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "n", "b", "neg");
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            builder.Circle(new(), 4, style);
            builder.Circle(new(), 1.5, style);
            builder.Circle(new(4, 0), 1, style.AsFilledMarker());

            double m = style.LineThickness * 0.5 + LabelMargin;
            _anchors[0] = new LabelAnchorPoint(new(-4 - m, 0), new(-1, 0));
            _anchors.Draw(builder, this, style);

            builder.ExtendPin(Pins["p"], style, 4);
            builder.ExtendPin(Pins["n"], style);
        }
    }
}