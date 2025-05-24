using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
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
                var style = builder.Style.Modify(Style);
                var textStyle = new FontSizeStyleModifier.Style(style, 0.8 * Styles.Style.DefaultFontSize * Scale);
                switch (Variants.Select("sdomain", "zdomain"))
                {
                    case 0:
                        var span = builder.TextFormatter.Format("s", textStyle);
                        builder.Text(span, new(), new());
                        break;

                    case 1:
                        span = builder.TextFormatter.Format("z^{-1}", textStyle);
                        builder.Text(span, new(), new());
                        break;

                    default:
                        span = builder.TextFormatter.Format("d", textStyle);
                        builder.Text(span, new(), new(0, -1));
                        builder.Line(new(-2, 0), new(2, 0), textStyle);
                        span = builder.TextFormatter.Format("dt", textStyle);
                        builder.Text(span, new(), new(0, 1));
                        break;
                }
                DrawLabels(builder, style);
            }
        }
    }
}
