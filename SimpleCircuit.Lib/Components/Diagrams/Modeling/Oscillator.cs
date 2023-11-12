namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("OSC", "An oscillator.", "Modeling", "source generator")]
    public class Oscillator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ModelingDrawable
        {
            /// <inheritdoc />
            public override string Type => "oscillator";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name, 10.0)
            {
            }

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
