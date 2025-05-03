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
                var textAppearance = Appearance.Clone();
                textAppearance.LineThickness = 0.1 * Scale;
                textAppearance.FontSize = 0.8 * AppearanceOptions.DefaultFontSize * Scale;
                switch (Variants.Select("sdomain", "zdomain"))
                {
                    case 0:
                        builder.Text("s", new(), new(), textAppearance);
                        break;

                    case 1:
                        builder.Text("z^{-1}", new(), new(), textAppearance);
                        break;

                    default:
                        builder.Text("d", new(), new(0, -1), textAppearance);
                        builder.Line(new(-2, 0), new(2, 0), textAppearance.CreatePathOptions());
                        builder.Text("dt", new(), new(0, 1), textAppearance);
                        break;
                }
                DrawLabels(builder);
            }
        }
    }
}
