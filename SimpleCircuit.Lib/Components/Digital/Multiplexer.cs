using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Digital;

/// <summary>
/// A multiplexer.
/// </summary>
[Drawable("MUX", "A multiplexer.", "Digital")]
public class Multiplexer : DrawableFactory
{
    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(1);

        /// <inheritdoc />
        public override string Type => "mux";

        [Description("The margin for labels to the edge.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        public Instance(string name)
            : base(name)
        {
            AddPin(new FixedOrientedPin("1", "The '1' input.", this, new(-5, -4), new(-1, 0)), "a", "1");
            AddPin(new FixedOrientedPin("0", "The '0' input.", this, new(-5, 4), new(-1, 0)), "b", "0");
            AddPin(new FixedOrientedPin("c", "The controlling input.", this, new(0, -6), new(0, -1)), "c");
            AddPin(new FixedOrientedPin("output", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "output");
        }

        public override PresenceResult Prepare(IPrepareContext context)
        {
            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    var style = context.Style.ModifyDashedDotted(this);
                    double m = style.LineThickness * 0.5 + LabelMargin;
                    _anchors[0] = new LabelAnchorPoint(new(0, 8 + m), new(0, 1));
                    break;
            }
            return base.Prepare(context);
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

            builder.ExtendPins(Pins, style);
            builder.Polygon([
                new(-5, -8),
                new(5, -4),
                new(5, 4),
                new(-5, 8)
            ], style);

            var textStyle = new FontSizeStyleModifier.Style(style, 0.8 * Style.DefaultFontSize * Scale);

            var span = builder.TextFormatter.Format("1", textStyle);
            builder.Text(span, new Vector2(-4, -4) - span.Bounds.Bounds.MiddleLeft, Vector2.UX, TextOrientationType.Transformed);

            span = builder.TextFormatter.Format("0", textStyle);
            builder.Text(span, new Vector2(-4, 4) - span.Bounds.Bounds.MiddleLeft, Vector2.UX, TextOrientationType.Transformed);

            _anchors.Draw(builder, this, style);
        }
    }
}
