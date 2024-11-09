using System;
using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("MIX", "A mixer", "Modeling", "x")]
    public class Mixer : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name)
        {
            /// <inheritdoc />
            public override string Type => "mixer";

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                base.Draw(builder);

                double s = Size * 0.5;
                if (!Variants.Contains(Square))
                    s /= Math.Sqrt(2.0);
                builder.Line(new(-s, -s), new(s, s));
                builder.Line(new(-s, s), new(s, -s));

                DrawLabels(builder);
            }
        }
    }
}
