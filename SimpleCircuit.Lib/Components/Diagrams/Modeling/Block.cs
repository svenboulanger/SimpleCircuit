using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

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
        }
    }
}
