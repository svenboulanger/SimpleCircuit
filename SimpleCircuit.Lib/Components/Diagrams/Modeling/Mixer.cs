using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;

namespace SimpleCircuit.Components.Diagrams.Modeling;

/// <summary>
/// A mixer.
/// </summary>
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
            var style = builder.Style.ModifyDashedDotted(this);

            double s = Size * 0.5;
            if (!Variants.Contains(Square))
                s /= Math.Sqrt(2.0);
            builder.Line(new(-s, -s), new(s, s), style);
            builder.Line(new(-s, s), new(s, -s), style);

            DrawLabels(builder, style);
        }
    }
}
