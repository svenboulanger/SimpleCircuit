using SimpleCircuit.Components.Labeling;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    /// <summary>
    /// A modeling block.
    /// </summary>
    [Drawable("BLOCK", "A generic block with text.", "Modeling", "box rectangle")]
    public class Block : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var result = new Instance(name);
            result.Variants.Add(ModelingDrawable.Square);
            return result;
        }

        private class Instance : ModelingDrawable
        {
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

                if (Variants.Contains(Square))
                    BoxLabelAnchorPoints.Default.Draw(drawing, this);
                else
                    EllipseLabelAnchorPoints.Default.Draw(drawing, this);
            }
        }
    }
}
