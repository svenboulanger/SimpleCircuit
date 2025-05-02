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

                switch (Variants.Select("sdomain", "zdomain"))
                {
                    case 0:
                        builder.Text("s", new(), new(), size: 0.8 * SvgBuilder.DefaultFontSize * Scale, options: new("small"));
                        break;

                    case 1:
                        builder.Text("z^{-1}", new(), new(), size: 0.8 * SvgBuilder.DefaultFontSize * Scale, options: new("small"));
                        break;

                    default:
                        var options = new GraphicOptions();
                        options.Style["stroke"] = Foreground;
                        options.Style["fill"] = "none";
                        options.Style["stroke-width"] = $"{(0.1 * Scale).ToSVG()}pt";
                        options.Style["stroke-linecap"] = "butt";
                        builder.Text("d", new(), new(0, -1), size: 0.8 * SvgBuilder.DefaultFontSize * Scale, options: new("small"));
                        builder.Line(new(-2, 0), new(2, 0), options);
                        builder.Text("dt", new(), new(0, 1), size: 0.8 * SvgBuilder.DefaultFontSize * Scale, options: new("small"));
                        break;
                }
                DrawLabels(builder);
            }
        }
    }
}
