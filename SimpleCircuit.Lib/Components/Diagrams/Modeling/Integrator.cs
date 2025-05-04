using SimpleCircuit.Components.Appearance;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.SvgPathData;
using System;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("INT", "An integrator.", "Modeling", "sum")]
    public class Integrator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var inst = new Instance(name);
            inst.Variants.Add(ModelingDrawable.Square);
            return inst;
        }

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name, 12.0)
        {
            private const string _pathData = @"M3 -8 c0 -2 -1 -3 -2 -3 c-1.5 0 -2 1 -2 3 S1 6 1 8 s-0.5 3 -2 3 c-1 0 -2 -1 -2 -3";

            /// <inheritdoc />
            public override string Type => "integrator";

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                base.Draw(builder);
                var textAppearance = new FixedTextAppearance(Appearance, 0.8 * AppearanceOptions.DefaultFontSize * Scale, 0.1 * Scale);
                switch (Variants.Select("sdomain", "zdomain"))
                {
                    case 0:
                        builder.Text("1", new(), new(0, -1), textAppearance);
                        builder.Line(new(-2, 0), new(2, 0), textAppearance);
                        builder.Text("s", new(), new(0, 1), textAppearance);
                        break;

                    case 1:
                        builder.Text("1", new(0, -1), new(0, -1), textAppearance);
                        builder.Line(new(-2, -1), new(2, -1), textAppearance);
                        builder.Text("z^{-1}", new(0, -1), new(0, 1), textAppearance);
                        break;

                    default:
                        // Draw an integral sign
                        builder.BeginTransform(new Transform(new(), Matrix2.Scale(Size * 0.3 / 11.0)));
                        builder.Path(b =>
                        {
                            var lexer = new SvgPathDataLexer(_pathData);
                            // b.WithTransform(new Transform(new(), Matrix2.Scale(Size * 0.3 / 11.0)));
                            SvgPathDataParser.Parse(lexer, b, null);
                        }, Appearance);
                        builder.EndTransform();
                        break;
                }
                DrawLabels(builder);
            }
        }
    }
}
