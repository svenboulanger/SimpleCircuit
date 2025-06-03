using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A fuse.
    /// </summary>
    [Drawable("FUSE", "A fuse.", "Wires")]
    public class Fuse : DrawableFactory
    {
        private const string _alt = "alt";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, IStandardizedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.American | Standards.European;

            /// <inheritdoc />
            public override string Type => "fuse";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "b", "n", "neg");
                _anchors = new(
                    new LabelAnchorPoint(new(0, -4), new(0, -1)),
                    new LabelAnchorPoint(new(0, 4), new(0, 1)));
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                switch (Variants.Select(Options.European, Options.American))
                {
                    case 0: DrawIEC(builder, style); break;
                    case 1:
                    default:
                        if (Variants.Contains(_alt))
                            DrawANSIalt(builder, style);
                        else
                            DrawANSI(builder, style);
                        break;
                }
            }
            private void DrawIEC(IGraphicsBuilder builder, IStyle style)
            {
                builder.ExtendPins(Pins, style);

                builder.Rectangle(-6, -3, 12, 6, style);
                builder.Path(b => b.MoveTo(new(-3.5, -3)).Line(new(0, 6)).MoveTo(new(3.5, -3)).Line(new(0, 6)), style);

                _anchors.Draw(builder, this, style);
            }
            private void DrawANSI(IGraphicsBuilder builder, IStyle style)
            {
                builder.ExtendPins(Pins, style);

                builder.Rectangle(-6, -3, 12, 6, style);
                builder.Line(new(-6, 0), new(6, 0), style);

                _anchors.Draw(builder, this, style);
            }
            private void DrawANSIalt(IGraphicsBuilder builder, IStyle style)
            {
                builder.Path(b => b
                    .MoveTo(new(-6, 0))
                    .CurveTo(new(-6, -1.65685424949), new(-4.65685424949, -3), new(-3, -3))
                    .CurveTo(new(-1.34314575051, -3), new(0, -1.65685424949), new())
                    .CurveTo(new(0, 1.65685424949), new(1.34314575051, 3), new(3, 3))
                    .CurveTo(new(4.65685424949, 3), new(6, 1.65685424949), new(6, 0)), style);

                _anchors.Draw(builder, this, style);
            }
        }
    }
}