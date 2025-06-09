using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A flip-flop.
    /// </summary>
    [Drawable("FF", "A general flip-flop.", "Digital", "edge trigger")]
    public class FlipFlop : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, IBoxDrawable
        {
            /// <inheritdoc />
            public override string Type => "flipflop";

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            /// <inheritdoc />
            Vector2 IBoxDrawable.TopLeft => new(-9, -12);

            /// <inheritdoc />
            Vector2 IBoxDrawable.Center => new();

            /// <inheritdoc />
            Vector2 IBoxDrawable.BottomRight => new(9, 12);

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

                var textStyle = new FontSizeStyleModifier.Style(style, Drawing.Styles.Style.DefaultFontSize);
                var span = builder.TextFormatter.Format("D", textStyle);
                var bounds = span.Bounds.Bounds;
                builder.Text(span, new Vector2(-8, -6) - bounds.MiddleLeft, TextOrientation.Transformed);

                span = builder.TextFormatter.Format("C", textStyle);
                bounds = span.Bounds.Bounds;
                builder.Text(span, new Vector2(-6, 6) - bounds.MiddleLeft, TextOrientation.Transformed);

                span = builder.TextFormatter.Format("Q", textStyle);
                bounds = span.Bounds.Bounds;
                builder.Text(span, new Vector2(8, -6) - bounds.MiddleRight, TextOrientation.Transformed);

                if (Pins["nq"].Connections > 0)
                {
                    span = builder.TextFormatter.Format("\\overline{Q}", textStyle);
                    bounds = span.Bounds.Bounds;
                    builder.Text(span, new Vector2(8, 6) - bounds.MiddleRight, TextOrientation.Transformed);
                }

                // Smaller text for asynchronous set and reset
                textStyle = new FontSizeStyleModifier.Style(style, 0.8 * Drawing.Styles.Style.DefaultFontSize);
                if (Pins["s"].Connections > 0)
                {
                    span = builder.TextFormatter.Format("set", textStyle);
                    bounds = span.Bounds.Bounds;
                    builder.Text(span, new Vector2(0, -11.5) - bounds.TopCenter, new(new Vector2(0, 1), TextOrientationTypes.Transformed));
                }
                if (Pins["r"].Connections > 0)
                {
                    span = builder.TextFormatter.Format("rst", textStyle);
                    bounds = span.Bounds.Bounds;
                    builder.Text(span, new Vector2(0, 11.5) - bounds.BottomCenter - new Vector2(bounds.Width * 0.5, 0), TextOrientation.Transformed);
                }

                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this, style);
            }
        }
    }
}