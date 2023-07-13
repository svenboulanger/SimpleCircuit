namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("DIFF", "A differentiator.", "Modeling")]
    public class Differentiator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var inst = new Instance(name);
            inst.Variants.Add(ModelingDrawable.Square);
            return inst;
        }

        private class Instance : ModelingDrawable
        {
            public override string Type => "differentiator";

            protected override double Size => 12;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name) : base(name)
            {
            }

            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);

                switch (Variants.Select("sdomain", "zdomain"))
                {
                    case 0:
                        drawing.Text("s", new(), new(), new("small"));
                        break;

                    case 1:
                        drawing.Text("z^{-1}", new(), new(), new("small"));
                        break;

                    default:
                        drawing.Text("d", new(), new(0, -1), new("small"));
                        drawing.Line(new(-2, 0), new(2, 0), new("text-stroke"));
                        drawing.Text("dt", new(), new(0, 1), new("small"));
                        break;
                }
            }
        }
    }
}
