using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire segment that has a bit more information.
    /// </summary>
    [Drawable("SEG", "A wire segment.", "Wires", "underground air tube inwall onwall arei")]
    public class Segment : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);
            private const string _underground = "underground";
            private const string _air = "air";
            private const string _tube = "tube";
            private const string _inwall = "inwall";
            private const string _onwall = "onwall";

            /// <inheritdoc />
            public override string Type => "segment";

            [Description("The number of tubes.")]
            [Alias("m")]
            public int Multiple { get; set; } = 1;

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
                Pins.Add(new FixedOrientedPin("input", "The input.", this, new(0, 0), new(-1, 0)), "i", "a", "in", "input");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(0, 0), new(1, 0)), "o", "b", "out", "output");
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                double m = style.LineThickness * 0.5 + LabelMargin;

                _anchors[0] = new LabelAnchorPoint(new(0, -m), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, m), new(0, 1));
                switch (Variants.Select(_underground, _air, _tube, _inwall, _onwall))
                {
                    case 0: DrawUnderground(builder, style); break;
                    case 1: DrawAir(builder, style); break;
                    case 2: DrawTube(builder, style); break;
                    case 3: DrawInWall(builder, style); break;
                    case 4: DrawOnWall(builder, style); break;
                }

                builder.ExtendPins(Pins, style, 4);
                _anchors.Draw(builder, this, style);
            }
            private void DrawUnderground(IGraphicsBuilder builder, IStyle style)
            {
                builder.Path(b => b
                    .MoveTo(new(-4, -5))
                    .Line(new(8, 0))
                    .MoveTo(new(-2.5, -3.5))
                    .Line(new(5, 0))
                    .MoveTo(new(-1, -2))
                    .Line(new(2, 0)), style);

                double m = style.LineThickness * 0.5 + LabelMargin;
                if (_anchors[0].Location.Y > -5 - m)
                    _anchors[0] = new LabelAnchorPoint(new(0, -5 - m), new(0, -1));
                if (_anchors[1].Location.Y < m)
                    _anchors[1] = new LabelAnchorPoint(new(0, m), new(0, 1));
            }
            private void DrawAir(IGraphicsBuilder builder, IStyle style)
            {
                builder.Circle(new(), 2, style);
                double m = style.LineThickness * 0.5 + LabelMargin;
                if (_anchors[0].Location.Y > -2 - m)
                    _anchors[0] = new LabelAnchorPoint(new(0, -2 - m), new(0, -1));
                if (_anchors[1].Location.Y < 2 + m)
                    _anchors[1] = new LabelAnchorPoint(new(0, 2 + m), new(0, 1));
            }
            private void DrawTube(IGraphicsBuilder builder, IStyle style)
            {
                builder.Circle(new(0, -3.5), 1.5, style);
                double m = style.LineThickness * 0.5 + LabelMargin;
                if (_anchors[0].Location.Y > -5 - m)
                    _anchors[0] = new LabelAnchorPoint(new(0, -5 - m), new(0, -1));
                if (_anchors[1].Location.Y < m)
                    _anchors[1] = new LabelAnchorPoint(new(0, m), new(0, 1));

                if (Multiple > 1)
                {
                    builder.Line(new(0, -3.5), new(2.1, -5.6), style);

                    var span = builder.TextFormatter.Format(Multiple.ToString(), style);
                    builder.Text(span, new Vector2(2.5, -5.1) + new Vector2(0.707, -0.707) * (style.FontSize + 1) * 0.5 - builder.CurrentTransform.Matrix.Inverse * span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.None);

                    _anchors[0] = new LabelAnchorPoint(new(0, -10 - m), new(0, -1));
                }
            }
            private void DrawInWall(IGraphicsBuilder builder, IStyle style)
            {
                builder.Polyline([
                    new(-3, -2),
                    new(-3, -5),
                    new(3, -5),
                    new(3, -2)
                ], style);
                builder.Line(new(0, -2), new(0, -5), style);

                double m = style.LineThickness * 0.5 + LabelMargin;
                if (_anchors[0].Location.Y > -5 - m)
                    _anchors[0] = new LabelAnchorPoint(new(0, -5 - m), new(0, -1));
                if (_anchors[1].Location.Y < m)
                    _anchors[1] = new LabelAnchorPoint(new(0, m), new(0, 1));
            }
            private void DrawOnWall(IGraphicsBuilder builder, IStyle style)
            {
                builder.Polyline([
                    new(-3, 5), 
                    new(-3, 2), 
                    new(3, 2), 
                    new(3, 5)
                ], style);
                builder.Line(new(0, 5), new(0, 2), style);

                double m = style.LineThickness * 0.5 + LabelMargin;
                if (_anchors[0].Location.Y > -1)
                    _anchors[0] = new LabelAnchorPoint(new(0, -m), new(0, -1));
                if (_anchors[1].Location.Y < 6)
                    _anchors[1] = new LabelAnchorPoint(new(0, 5 + m), new(0, 1));
            }
        }
    }
}