using SimpleCircuit.Components.Builders;
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

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name, 12.0)
        {
            /// <inheritdoc />
            public override string Type => "block";

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                base.Draw(builder);

                if (Variants.Contains(Square))
                    BoxLabelAnchorPoints.Default.Draw(builder, this);
                else
                    EllipseLabelAnchorPoints.Default.Draw(builder, this);
            }
        }
    }
}
