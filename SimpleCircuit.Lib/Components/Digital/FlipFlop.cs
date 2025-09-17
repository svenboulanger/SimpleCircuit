using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A flip-flop.
    /// </summary>
    [Drawable("FF", "A general flip-flop.", "Digital", "edge trigger", labelCount: 2)]
    public class FlipFlop : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);

            /// <inheritdoc />
            public override string Type => "flipflop";

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
                Pins.Add(new FixedOrientedPin("data", "The data pin.", this, new(-9, -6), new(-1, 0)), "d", "data");
                Pins.Add(new FixedOrientedPin("clock", "The clock pin.", this, new(-9, 6), new(-1, 0)), "c", "clock");
                Pins.Add(new FixedOrientedPin("reset", "The reset pin.", this, new(0, 12), new(0, 1)), "r", "rst", "reset");
                Pins.Add(new FixedOrientedPin("set", "The set pin.", this, new(0, -12), new(0, -1)), "s", "set");
                Pins.Add(new FixedOrientedPin("nq", "The inverted output pin.", this, new(9, 6), new(1, 0)), "nq", "qn");
                Pins.Add(new FixedOrientedPin("q", "The output pin.", this, new(9, -6), new(1, 0)), "q");
            }

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        var style = context.Style.ModifyDashedDotted(this);
                        double m = style.LineThickness * 0.5 + LabelMargin;
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
                builder.ExtendPins(Pins, style, 2, "d", "c", "q");

                // Body
                builder.Rectangle(-9, -12, 18, 24, style, new());

                // Clock thingy
                builder.Polyline([
                    new Vector2(-9, 4),
                    new Vector2(-7, 6),
                    new Vector2(-9, 8)
                ], style);

                var textStyle = new FontSizeStyleModifier.Style(style, Style.DefaultFontSize);
                var span = builder.TextFormatter.Format("D", textStyle);
                builder.Text(span, new Vector2(-8 - span.Bounds.Bounds.Left, -6 + textStyle.FontSize * 0.5), Vector2.UX, TextOrientationType.Transformed);

                span = builder.TextFormatter.Format("C", textStyle);
                builder.Text(span, new Vector2(-6 - span.Bounds.Bounds.Left, 6 + textStyle.FontSize * 0.5), Vector2.UX, TextOrientationType.Transformed);

                span = builder.TextFormatter.Format("Q", textStyle);
                builder.Text(span, new Vector2(8 - span.Bounds.Bounds.Right, -6 + textStyle.FontSize * 0.5), Vector2.UX, TextOrientationType.Transformed);

                if (Pins["nq"].Connections > 0)
                {
                    span = builder.TextFormatter.Format("\\overline{Q}", textStyle);
                    builder.Text(span, new Vector2(8 - span.Bounds.Bounds.Right, 6 + textStyle.FontSize * 0.5), Vector2.UX, TextOrientationType.Transformed);
                }

                // Smaller text for asynchronous set and reset
                textStyle = new FontSizeStyleModifier.Style(style, 0.8 * Style.DefaultFontSize);
                if (Pins["s"].Connections > 0)
                {
                    span = builder.TextFormatter.Format("set", textStyle);
                    builder.Text(span, new Vector2(0, -11.5) - span.Bounds.Bounds.TopCenter, Vector2.UX, TextOrientationType.Transformed);
                }
                if (Pins["r"].Connections > 0)
                {
                    span = builder.TextFormatter.Format("rst", textStyle);
                    builder.Text(span, new Vector2(0, 11.5) - span.Bounds.Bounds.BottomCenter, Vector2.UX, TextOrientationType.Transformed);
                }

                _anchors.Draw(builder, this, style);
            }
        }
    }
}