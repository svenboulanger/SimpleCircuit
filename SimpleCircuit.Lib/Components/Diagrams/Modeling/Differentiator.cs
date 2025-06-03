using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    /// <summary>
    /// A differentiator.
    /// </summary>
    [Drawable("DIFF", "A differentiator.", "Modeling", "derivative")]
    public class Differentiator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var inst = new Instance(name);
            inst.Variants.Add(ModelingDrawable.Square);
            return inst;
        }

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name, 12.0)
        {
            /// <inheritdoc />
            public override string Type => "differentiator";

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                base.Draw(builder);
                var style = builder.Style.ModifyDashedDotted(this);
                var textStyle = new FontSizeStyleModifier.Style(style, 0.8 * Styles.Style.DefaultFontSize * Scale);
                switch (Variants.Select("sdomain", "zdomain"))
                {
                    case 0:
                        // 's'
                        var span = builder.TextFormatter.Format("s", textStyle);
                        var bounds = span.Bounds.Bounds;
                        builder.Text(span, new(-bounds.Left - bounds.Width * 0.5, -bounds.Top - bounds.Height * 0.5), new());
                        break;

                    case 1:
                        // 'z'
                        span = builder.TextFormatter.Format("z", textStyle);
                        bounds = span.Bounds.Bounds;
                        builder.Text(span, new(-bounds.Left - bounds.Width * 0.5, -bounds.Top - bounds.Height * 0.5), new());
                        break;

                    default:
                        // d/dt
                        span = builder.TextFormatter.Format("d", textStyle);
                        bounds = span.Bounds.Bounds;
                        builder.Text(span, new(-bounds.Left - bounds.Width * 0.5, -bounds.Bottom - 1), new());
                        builder.Line(new(-2, 0), new(2, 0), textStyle);
                        span = builder.TextFormatter.Format("dt", textStyle);
                        bounds = span.Bounds.Bounds;
                        builder.Text(span, new(-bounds.Left - bounds.Width * 0.5, -bounds.Top + 1), new());
                        break;
                }
                DrawLabels(builder, style);
            }
        }
    }
}
