using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Styles;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    /// <summary>
    /// A subtraction.
    /// </summary>
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
            protected override void Draw(IGraphicsBuilder builder)
            {
                base.Draw(builder);
                var style = builder.Style.ModifyDashedDotted(this);

                double s = Size * 0.3;
                builder.Line(new(-s, 0), new(s, 0), style);
                DrawLabels(builder, style);
            }
        }
    }
}
