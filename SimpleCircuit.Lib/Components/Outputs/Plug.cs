using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A wall plug.
    /// </summary>
    [Drawable("WP", "A wall plug.", "Outputs", "earth child proof sealed", labelCount: 2)]
    public class Plug : DrawableFactory
    {
        private const string _earth = "earth";
        private const string _sealed = "sealed";
        private const string _child = "child";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);

            /// <inheritdoc />
            public override string Type => "plug";

            /// <summary>
            /// The distance from the label to the symbol.
            /// </summary>
            [Description("The margin for labels.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            [Description("The multiplicity of the wall plug.")]
            [Alias("m")]
            public int Multiple { get; set; } = 1;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(), new(-1, 0)), "in", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(), new(1, 0)), "out", "b");
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                double m = style.LineThickness * 0.5 + LabelMargin;

                switch (Variants.Select(_child))
                {
                    case 0:
                        builder.Path(b => b.MoveTo(new(4, -6)).LineTo(new(4, -4)).ArcTo(4, 4, 0, true, false, new(4, 4)).LineTo(new(4, 6)), style.AsStroke());
                        _anchors[0] = new LabelAnchorPoint(new(4, -6 - m), new(0, -1));
                        _anchors[1] = new LabelAnchorPoint(new(4, 6 + m), new(0, 1));
                        break;

                    default:
                        builder.Path(b => b.MoveTo(new(4, -4)).ArcTo(4, 4, 0, true, false, new(4, 4)), style.AsStroke());
                        _anchors[0] = new LabelAnchorPoint(new(4, -4 - m), new(0, -1));
                        _anchors[1] = new LabelAnchorPoint(new(4, 4 + m), new(0, 1));
                        break;
                }

                if (Variants.Contains(_earth))
                {
                    builder.Line(new(0, 4), new(0, -4), style);

                    if (Variants.Contains(_sealed))
                    {
                        var span = builder.TextFormatter.Format("h", style);
                        builder.Text(span, new Vector2(0.5, 4 + style.FontSize) - builder.CurrentTransform.Matrix.Inverse * span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.None);
                        _anchors[1] = new LabelAnchorPoint(new(4, 7 + style.FontSize), new(0, 1));
                    }
                }
                else if (Variants.Contains(_sealed))
                {
                    var span = builder.TextFormatter.Format("h", style);
                    builder.Text(span, new Vector2(0.5, 2.5 + style.FontSize) - builder.CurrentTransform.Matrix.Inverse * span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.None);
                    _anchors[1] = new LabelAnchorPoint(new(4, 5.5 + style.FontSize), new(0, 1));
                }

                if (Multiple > 1)
                {
                    builder.Line(new(2.6, -1.4), new(-0.2, -4.2), style);

                    var span = builder.TextFormatter.Format(Multiple.ToString(), style);
                    builder.Text(span, new Vector2(-0.2, -4.2) + new Vector2(-0.707, -0.707) * style.FontSize - builder.CurrentTransform.Matrix.Inverse * span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.None);
                }

                builder.ExtendPin(Pins["a"], style);
                _anchors.Draw(builder, this, style);
            }
        }
    }
}