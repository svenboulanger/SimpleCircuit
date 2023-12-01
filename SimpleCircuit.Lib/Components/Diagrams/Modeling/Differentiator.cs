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
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);

                switch (Variants.Select("sdomain", "zdomain"))
                {
                    case 0:
                        drawing.Text("s", new(), new(), size: 0.8 * SvgDrawing.DefaultFontSize * Scale, options: new("small"));
                        break;

                    case 1:
                        drawing.Text("z^{-1}", new(), new(), size: 0.8 * SvgDrawing.DefaultFontSize * Scale, options: new("small"));
                        break;

                    default:
                        drawing.Text("d", new(), new(0, -1), size: 0.8 * SvgDrawing.DefaultFontSize * Scale, options: new("small"));
                        drawing.Line(new(-2, 0), new(2, 0), new() { Style = $"stroke-width: {(0.1 * Scale).ToCoordinate()}pt;"});
                        drawing.Text("dt", new(), new(0, 1), size: 0.8 * SvgDrawing.DefaultFontSize * Scale, options: new("small"));
                        break;
                }
                DrawLabels(drawing);
            }
        }
    }
}
