using SimpleCircuit.Components.Builders;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.SvgPathData;
using System;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("ADD", "Addition.", "Modeling", "plus sum")]
    public class Addition : DrawableFactory
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
            private const string _pathData = @"M0.75 -1 l-1.5 0 1 1 -1 1 1.5 0";

            /// <inheritdoc />
            public override string Type => "addition";

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                base.Draw(builder);

                switch (Variants.Select("sigma"))
                {
                    case 0:
                        // Draw a sigma
                        builder.BeginTransform(new Transform(new(), Matrix2.Scale(0.2 * Size)));
                        builder.Path(b =>
                        {
                            var lexer = new SvgPathDataLexer(_pathData);
                            SvgPathDataParser.Parse(lexer, b, null);
                        });
                        builder.EndTransform();
                        break;

                    default:
                        double s = Size * 0.3;
                        builder.Line(new(-s, 0), new(s, 0), Appearance);
                        builder.Line(new(0, -s), new(0, s), Appearance);
                        break;
                }

                DrawLabels(builder);
            }
        }
    }
}
