using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    /// <summary>
    /// A split block.
    /// </summary>
    [Drawable("SPLIT", "A block with a split line.", "Modeling")]
    public class SplitBlock : DrawableFactory
    {
        protected override IDrawable Factory(string key, string name)
        {
            var result = new Instance(name);
            result.Variants.Add(ModelingDrawable.Square);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name, 12.0)
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            /// <inheritdoc />
            public override string Type => "split";

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                base.Draw(builder);
                double s = Size * 0.5;
                if (!Variants.Contains(Square))
                    s *= 0.70710678118;
                builder.Line(new(-s, s), new(s, -s), Appearance);

                _anchors[0] = new LabelAnchorPoint(new(-s * 0.5, -s * 0.5), new(), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(s * 0.5, s * 0.5), new(), Appearance);
                if (Variants.Contains(Square))
                    new AggregateAnchorPoints<IBoxDrawable>(_anchors,
                        new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1)).Draw(builder, this);
                else
                    new AggregateAnchorPoints<IEllipseDrawable>(_anchors, 
                        new OffsetAnchorPoints<IEllipseDrawable>(EllipseLabelAnchorPoints.Default, 1)).Draw(builder, this);
            }
        }
    }
}
