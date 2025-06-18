using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.SvgPathData;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Drawing.Builders;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    /// <summary>
    /// An integrator.
    /// </summary>
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
                var style = builder.Style.ModifyDashedDotted(this);
                var textStyle = new FontSizeStyleModifier.Style(style, 0.8 * Drawing.Styles.Style.DefaultFontSize * Scale);
                switch (Variants.Select("sdomain", "zdomain"))
                {
                    case 0:
                        // 1/s
                        var span = builder.TextFormatter.Format("1", textStyle);
                        var bounds = span.Bounds.Bounds;
                        builder.Text(span, new(-bounds.Left - bounds.Width * 0.5, -bounds.Bottom - 1), Vector2.UX, TextOrientationTypes.Transformed);
                        builder.Line(new(-2, 0), new(2, 0), textStyle);
                        span = builder.TextFormatter.Format("s", textStyle);
                        bounds = span.Bounds.Bounds;
                        builder.Text(span, new(-bounds.Left - bounds.Width * 0.5, -bounds.Top + 1), Vector2.UX, TextOrientationTypes.Transformed);
                        break;

                    case 1:
                        // 1/z^-1
                        span = builder.TextFormatter.Format("1", textStyle);
                        bounds = span.Bounds.Bounds;
                        builder.Text(span, new(-bounds.Left - bounds.Width * 0.5, -bounds.Bottom - 2), Vector2.UX, TextOrientationTypes.Transformed);
                        builder.Line(new(-2, -1), new(2, -1), textStyle);
                        span = builder.TextFormatter.Format("z^{-1}", textStyle);
                        bounds = span.Bounds.Bounds;
                        builder.Text(span, new(-bounds.Left - bounds.Width * 0.5, -bounds.Top), Vector2.UX, TextOrientationTypes.Transformed);
                        break;

                    default:
                        // Draw an integral sign
                        builder.BeginTransform(new Transform(new(), Matrix2.Scale(Size * 0.3 / 11.0)));
                        builder.Path(b =>
                        {
                            var lexer = new SvgPathDataLexer(_pathData);
                            // b.WithTransform(new Transform(new(), Matrix2.Scale(Size * 0.3 / 11.0)));
                            SvgPathDataParser.Parse(lexer, b, null);
                        }, style);
                        builder.EndTransform();
                        break;
                }
                DrawLabels(builder, style);
            }
        }
    }
}
