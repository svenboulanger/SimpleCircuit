using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Diagrams.Modeling;

/// <summary>
/// An oscillator.
/// </summary>
[Drawable("OSC", "An oscillator.", "Modeling", "source generator")]
public class Oscillator : DrawableFactory
{
    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    /// <summary>
    /// Creates a new <see cref="Instance"/>.
    /// </summary>
    /// <param name="name">The name.</param>
    private class Instance(string name) : ModelingDrawable(name, 10.0)
    {
        /// <inheritdoc />
        public override string Type => "oscillator";

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            base.Draw(builder);
            var style = builder.Style.ModifyDashedDotted(this);

            builder.AC(style, new(), Size * 0.25);
            DrawLabels(builder, style);
        }
    }
}
