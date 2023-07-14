using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.SvgPathData;
using System;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("INT", "An integrator.", "Modeling")]
    public class Integrator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var inst = new Instance(name);
            inst.Variants.Add(ModelingDrawable.Square);
            return inst;
        }

        private class Instance : ModelingDrawable
        {
            private const string _pathData = @"M3 -8 c0 -2 -1 -3 -2 -3 c-1.5 0 -2 1 -2 3 S1 6 1 8 s-0.5 3 -2 3 c-1 0 -2 -1 -2 -3";

            /// <inheritdoc />
            public override string Type => "integrator";

            /// <inheritdoc />
            protected override double Size => 12;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name) : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);

                switch (Variants.Select("sdomain", "zdomain"))
                {
                    case 0:
                        drawing.Text("1", new(), new(0, -1), new("small"));
                        drawing.Line(new(-2, 0), new(2, 0), new("text-stroke"));
                        drawing.Text("s", new(), new(0, 1), new("small"));
                        break;

                    case 1:
                        drawing.Text("1", new(0, -1), new(0, -1), new("small"));
                        drawing.Line(new(-2, -1), new(2, -1), new("text-stroke"));
                        drawing.Text("z^{-1}", new(0, -1), new(0, 1), new("small"));
                        break;

                    default:
                        // Draw an integral sign
                        drawing.Path(b =>
                        {
                            var lexer = new SvgPathDataLexer(_pathData.AsMemory());
                            b.WithTransform(new Transform(new(), Matrix2.Scale(Size * 0.3 / 11.0)));
                            SvgPathDataParser.Parse(lexer, b, null);
                        });
                        break;
                }
            }
        }
    }
}
