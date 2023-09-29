namespace SimpleCircuit.Components.Diagrams.Modeling
{
    /// <summary>
    /// A modeling block.
    /// </summary>
    [Drawable("BLOCK", "A generic block with text", "Modeling")]
    public class Block : DrawableFactory
    {
        /// <inheritdoc />
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
            public override string Type => "block";

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
                Labels.SetDefaultPin(0, location: new(), expand: new());
                Labels.Draw(drawing);
            }
        }
    }
}
