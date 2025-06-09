using System;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Styles;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("CIRC", "A circulator.", "Modeling", "rotate")]
    public class Circulator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name, 12.0)
        {
            /// <inheritdoc />
            public override string Type => "circulator";

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                base.Draw(builder);
                var style = builder.Style.ModifyDashedDotted(this);

                builder.Path(b =>
                {
                    double r = Size * 0.25;
                    double c = Math.Cos(-Math.PI * 0.8) * r;
                    double s = Math.Sin(-Math.PI * 0.8) * r;
                    b.MoveTo(new(c, s));
                    b.ArcTo(r, r, 0.0, true, true, new(c, -s));
                    b.Line(new(0.5, 2));
                    b.MoveTo(new(c, -s));
                    b.Line(new(2, 0));
                }, style);
                DrawLabels(builder, style);
            }
        }
    }
}
