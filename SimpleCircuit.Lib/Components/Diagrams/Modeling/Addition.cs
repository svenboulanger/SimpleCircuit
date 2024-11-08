﻿using SimpleCircuit.Drawing;
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
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);

                switch (Variants.Select("sigma"))
                {
                    case 0:
                        // Draw a sigma
                        drawing.BeginTransform(new Transform(new(), Matrix2.Scale(0.2 * Size)));
                        drawing.Path(b =>
                        {
                            var lexer = new SvgPathDataLexer(_pathData.AsMemory());
                            SvgPathDataParser.Parse(lexer, b, null);
                        });
                        drawing.EndTransform();
                        break;

                    default:
                        double s = Size * 0.3;
                        drawing.Line(new(-s, 0), new(s, 0));
                        drawing.Line(new(0, -s), new(0, s));
                        break;
                }

                DrawLabels(drawing);
            }
        }
    }
}
