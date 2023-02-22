namespace SimpleCircuit.Components.Modeling
{
    [Drawable("ADD", "Addition", "Modeling")]
    public class Addition : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ModelingDrawable
        {
            /// <inheritdoc />
            public override string Type => "add";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);
                double s = Size * 0.3;
                drawing.Line(new(-s, 0), new(s, 0));
                drawing.Line(new(0, -s), new(0, s));
            }
        }
    }
}
