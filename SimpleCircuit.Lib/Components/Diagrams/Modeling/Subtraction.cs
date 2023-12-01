namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("SUB", "Subtraction.", "Modeling", "minus difference")]
    internal class Subtraction : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new <see cref="Instance"/>
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name)
        {
            /// <inheritdoc />
            public override string Type => "subtraction";

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);
                double s = Size * 0.3;
                drawing.Line(new(-s, 0), new(s, 0));
                DrawLabels(drawing);
            }
        }
    }
}
