using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Digital;

/// <summary>
/// A latch.
/// </summary>
[Drawable("LATCH", "A general latch.", "Digital", "level trigger", labelCount: 2)]
public class Latch : DrawableFactory
{
    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(2);

        /// <inheritdoc />
        public override string Type => "latch";

        [Description("The margin for labels to the edge.")]
        [Alias("lm")]
        public double OuterMargin { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        public Instance(string name)
            : base(name)
        {
            AddPin(new FixedOrientedPin("set", "The set pin.", this, new(-9, -6), new(-1, 0)), "s", "set");
            AddPin(new FixedOrientedPin("reset", "The reset pin.", this, new(-9, 6), new(-1, 0)), "r", "reset");
            AddPin(new FixedOrientedPin("nq", "The inverted output pin.", this, new(9, 6), new(1, 0)), "nq", "qn");
            AddPin(new FixedOrientedPin("q", "The output pin.", this, new(9, -6), new(1, 0)), "q");
        }

        /// <inheritdoc />
        public override PresenceResult Prepare(IPrepareContext context)
        {
            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    var style = context.Style.ModifyDashedDotted(this);
                    double m = style.LineThickness * 0.5 + OuterMargin;
                    _anchors[0] = new LabelAnchorPoint(new(0, -12 - m), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 12 + m), new(0, 1));
                    break;
            }
            return base.Prepare(context);
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            builder.ExtendPins(Pins, style, 2, "s", "r", "q");

            // Body
            builder.Rectangle(-9, -12, 18, 24, style, new());

            // Labels
            var textStyle = new FontSizeStyleModifier.Style(style, Style.DefaultFontSize);

            var span = builder.TextFormatter.Format("S", textStyle);
            builder.Text(span, new Vector2(-8 - span.Bounds.Bounds.Left, -6 + textStyle.FontSize * 0.5), Vector2.UX, TextOrientationType.Transformed);

            span = builder.TextFormatter.Format("R", textStyle);
            builder.Text(span, new Vector2(-8 - span.Bounds.Bounds.Left, 6 + textStyle.FontSize * 0.5), Vector2.UX, TextOrientationType.Transformed);

            span = builder.TextFormatter.Format("Q", textStyle);
            builder.Text(span, new Vector2(8 - span.Bounds.Bounds.Right, -6 + textStyle.FontSize * 0.5), Vector2.UX, TextOrientationType.Transformed);

            if (Pins["nq"].Connections > 0)
            {
                span = builder.TextFormatter.Format("\\overline{Q}", textStyle);
                builder.Text(span, new Vector2(8 - span.Bounds.Bounds.Right, 6 + textStyle.FontSize * 0.5), Vector2.UX, TextOrientationType.Transformed);
            }

            _anchors.Draw(builder, this, style);
        }
    }
}
