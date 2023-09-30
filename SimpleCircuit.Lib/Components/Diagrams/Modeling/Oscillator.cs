namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("OSC", "An oscillator", "Modeling")]
    public class Oscillator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ModelingDrawable, ILabeled
        {
            protected override double Size => 10;

            /// <inheritdoc />
            public override string Type => "oscillator";

            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name) : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);
                drawing.AC(new(), Size * 0.25);

                Labels.BoxedLabel(Variants, new(-Size * 0.5, -Size * 0.5), new(Size * 0.5, Size * 0.5), 1, -1, 1);
                Labels.Draw(drawing);
            }
        }
    }
}
