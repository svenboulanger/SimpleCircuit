namespace SimpleCircuit.Components.Diagrams.Modeling
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
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            protected override double Size => 12;

            /// <inheritdoc />
            public override string Type => "split";

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
                if (!Variants.Contains(Square))
                    s *= 0.70710678118;
                drawing.Line(new(-s, s), new(s, -s));

                Labels.SetDefaultPin(0, location: new(-s * 0.5, -s * 0.5), new());
                Labels.SetDefaultPin(1, location: new(s * 0.5, s * 0.5), new());
                Labels.Draw(drawing);
            }
        }
    }
}
