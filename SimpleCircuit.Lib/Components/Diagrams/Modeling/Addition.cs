using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Parser.SvgPathData;

namespace SimpleCircuit.Components.Diagrams.Modeling;

/// <summary>
/// A modeling diagram block that represents addition.
/// </summary>
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
            var style = builder.Style.ModifyDashedDotted(this);

            switch (Variants.Select("sigma"))
            {
                case 0:
                    // Draw a sigma
                    builder.BeginTransform(new Transform(new(), Matrix2.Scale(0.2 * Size)));
                    builder.Path(b =>
                    {
                        var lexer = new SvgPathDataLexer(_pathData);
                        SvgPathDataParser.Parse(lexer, b, null);
                    }, style.AsStrokeMarker(Style.DefaultLineThickness));
                    builder.EndTransform();
                    break;

                default:
                    // Plus
                    double s = Size * 0.3;
                    builder.Line(new(-s, 0), new(s, 0), style.AsStrokeMarker(Style.DefaultLineThickness));
                    builder.Line(new(0, -s), new(0, s), style.AsStrokeMarker(Style.DefaultLineThickness));
                    break;
            }

            DrawLabels(builder, style);
        }
    }
}
