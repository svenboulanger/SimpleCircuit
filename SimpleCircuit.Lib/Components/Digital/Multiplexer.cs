using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A multiplexer.
    /// </summary>
    [Drawable("MUX", "A multiplexer.", "Digital")]
    public class Multiplexer : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, IBoxDrawable
        {
            /// <inheritdoc />
            public override string Type => "mux";

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            /// <inheritdoc />
            Vector2 IBoxDrawable.TopLeft => new(-5, -8);

            /// <inheritdoc />
            Vector2 IBoxDrawable.Center => new();

            /// <inheritdoc />
            Vector2 IBoxDrawable.BottomRight => new(5, 8);

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("1", "The '1' input.", this, new(-5, -4), new(-1, 0)), "a", "1");
                Pins.Add(new FixedOrientedPin("0", "The '0' input.", this, new(-5, 4), new(-1, 0)), "b", "0");
                Pins.Add(new FixedOrientedPin("c", "The controlling input.", this, new(0, -6), new(0, -1)), "c");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "output");
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

                var textStyle = new FontSizeStyleModifier.Style(style, 0.8 * Styles.Style.DefaultFontSize * Scale);

                var span = builder.TextFormatter.Format("1", textStyle);
                builder.Text(span, new Vector2(-4, -4) - span.Bounds.Bounds.MiddleLeft, TextOrientation.Transformed);

                span = builder.TextFormatter.Format("0", textStyle);
                builder.Text(span, new Vector2(-4, 4) - span.Bounds.Bounds.MiddleLeft, TextOrientation.Transformed);

                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this, style);
            }
        }
    }
}
