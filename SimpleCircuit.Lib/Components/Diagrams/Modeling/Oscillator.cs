namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("OSC", "An oscillator.", "Modeling", "source generator")]
    public class Oscillator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name, 10.0)
        {
            /// <inheritdoc />
            public override string Type => "oscillator";

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);
                drawing.AC(new(), Size * 0.25);
                DrawLabels(drawing);
            }
        }
    }
}
