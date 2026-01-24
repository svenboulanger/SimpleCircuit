using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Outputs;

/// <summary>
/// A motor.
/// </summary>
[Drawable("MOTOR", "A motor.", "Outputs", labelCount: 2)]
public class Motor : DrawableFactory
{
    private const string _signs = "signs";

    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(2);

        /// <inheritdoc />
        public override string Type => "motor";

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
            AddPin(new FixedOrientedPin("positive", "The positive pin.", this, new(-5, 0), new(-1, 0)), "p", "pos", "a");
            AddPin(new FixedOrientedPin("negative", "The negative pin.", this, new(5, 0), new(1, 0)), "n", "neg", "b");
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            double m = style.LineThickness * 0.5 + LabelMargin;
            _anchors[0] = new LabelAnchorPoint(new(0, -5 - m), new(0, -1));
            _anchors[1] = new LabelAnchorPoint(new(0, 5 + m), new(0, 1));

            if (!Variants.Contains(Options.Arei))
                builder.ExtendPins(Pins, style);
            builder.Circle(new(), 5, style);

            var span = builder.TextFormatter.Format("M", style);
            builder.Text(span, builder.CurrentTransform.Matrix.Inverse * -span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.None);

            if (Variants.Contains(_signs))
                builder.Signs(new(-6, -4), new(6, -4), style, upright: true);
            _anchors.Draw(builder, this, style);
        }
    }
}