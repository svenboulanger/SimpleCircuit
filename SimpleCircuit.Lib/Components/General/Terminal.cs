using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A factory for a terminal.
    /// </summary>
    [Drawable("T", "A common terminal symbol.", "General", "in input out output other pad square")]
    public class TerminalFactory : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(1);

            /// <inheritdoc />
            public override string Type => "terminal";

            /// <summary>
            /// The distance from the label to the symbol.
            /// </summary>
            [Description("The margin for labels.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("p", "The pin.", this, new Vector2(), new Vector2(1, 0)), "p", "a", "o", "i");
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                builder.ExtendPins(Pins, style, 4);
                double m = style.LineThickness * 0.5 + LabelMargin;

                switch (Variants.Select("input", "in", "output", "out", "inout", "other", "pad", "square", "none"))
                {
                    case 0:
                    case 1:
                        // Input
                        builder.Polygon([
                            new(-5, -2),
                            new(-2, -2),
                            new(),
                            new(-2, 2),
                            new(-5, 2)
                        ], style);

                        _anchors[0] = new LabelAnchorPoint(new(-5 - m, 0), new(-1, 0));
                        break;

                    case 2:
                    case 3:
                        // output
                        builder.Polygon([
                            new(-5, 0),
                            new(-3, -2),
                            new(0, -2),
                            new(0, 2),
                            new(-3, 2)
                        ], style);
                        _anchors[0] = new LabelAnchorPoint(new(-5 - m, 0), new(-1, 0));
                        break;

                    case 4:
                        // inout
                        builder.Polygon(
                        [
                            new(-7, 0),
                            new(-5, -2),
                            new(-2, -2),
                            new(),
                            new(-2, 2),
                            new(-5, 2)
                        ], style);
                        _anchors[0] = new LabelAnchorPoint(new(-7 - m, 0), new(-1, 0));
                        break;

                    case 5:
                        // other
                        builder.Polygon([
                            new(-5, -2),
                            new(0, -2),
                            new(0, 2),
                            new(-5, 2)
                        ], style);
                        _anchors[0] = new LabelAnchorPoint(new(-5 - m, 0), new(-1, 0));
                        break;

                    case 6:
                        // pad
                        builder.Rectangle(-4, -2, 4, 4, style);
                        builder.Cross(new(-2, 0), 4, style);
                        _anchors[0] = new LabelAnchorPoint(new(-4 - m, 0), new(-1, 0));
                        break;

                    case 7:
                        // square
                        builder.Rectangle(-4, -2, 4, 4, style);
                        _anchors[0] = new LabelAnchorPoint(new(-4 - m, 0), new(-1, 0));
                        break;

                    case 8:
                        // None
                        _anchors[0] = new LabelAnchorPoint(new(-m, 0), new(-1, 0));
                        break;

                    default:
                        builder.Circle(new Vector2(-1.5, 0), 1.5, style);
                        _anchors[0] = new LabelAnchorPoint(new(-3 - m, 0), new(-1, 0));
                        break;
                }
                _anchors.Draw(builder, this, style);
            }
        }
    }
}
