using SimpleCircuit.Parser.SvgPathData;
using System;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("ADD", "Addition", "Modeling")]
    public class Addition : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ModelingDrawable
        {
            private const string _pathData = @"M0.75 -1 l-1.5 0 1 1 -1 1 1.5 0";

            /// <inheritdoc />
            public override string Type => "addition";

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

                switch (Variants.Select("sigma"))
                {
                    case 0:
                        // Draw a sigma
                        drawing.Path(b =>
                        {
                            var lexer = new SvgPathDataLexer(_pathData.AsMemory());
                            b.WithRelativeModifier(v => v * 0.2 * Size).WithAbsoluteModifier(v => v * 0.2 * Size);
                            SvgPathDataParser.Parse(lexer, b, null);
                        });
                        break;

                    default:
                        double s = Size * 0.3;
                        drawing.Line(new(-s, 0), new(s, 0));
                        drawing.Line(new(0, -s), new(0, s));
                        break;
                }
            }
        }
    }
}
