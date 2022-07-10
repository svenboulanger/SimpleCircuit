namespace SimpleCircuit.Components.Modeling
{
    /// <summary>
    /// A split block.
    /// </summary>
    [Drawable("SPLIT", "A block with a split line", "Modeling")]
    public class SplitBlock : DrawableFactory
    {
        protected override IDrawable Factory(string key, string name)
        {
            var result = new Instance(name);
            result.Variants.Add(ModelingDrawable.Square);
            return result;
        }

        private class Instance : ModelingDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new(2);

            protected override double Size => 12;

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
                double s = Size * 0.5;
                drawing.Line(new(-s, s), new(s, -s));
                drawing.Text(Labels[0], new(-s * 0.5, -s * 0.5), new());
                drawing.Text(Labels[1], new(s * 0.5, s * 0.5), new());
            }
        }
    }
}
