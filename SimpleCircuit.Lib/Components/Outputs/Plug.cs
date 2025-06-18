using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A wall plug.
    /// </summary>
    [Drawable("WP", "A wall plug.", "Outputs", "earth child proof sealed")]
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
            private readonly CustomLabelAnchorPoints _anchors;

            /// <inheritdoc />
            public override string Type => "plug";

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
                _anchors = new(
                    new LabelAnchorPoint(new(6, -1), new(1, -1)));
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);

                switch (Variants.Select(_child))
                {
                    case 0:
                        builder.Path(b => b.MoveTo(new(4, -6)).LineTo(new(4, -4)).ArcTo(4, 4, 0, true, false, new(4, 4)).LineTo(new(4, 6)), style);
                        break;

                    default:
                        builder.Path(b => b.MoveTo(new(4, -4)).ArcTo(4, 4, 0, true, false, new(4, 4)), style);
                        break;
                }

                if (Variants.Contains(_earth))
                {
                    builder.Line(new(0, 4), new(0, -4), style);

                    if (Variants.Contains(_sealed))
                    {
                        var span = builder.TextFormatter.Format("h", style);
                        builder.Text(span, new Vector2(0.5, 4 + style.FontSize) - builder.CurrentTransform.Matrix.Inverse * span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.Upright);
                    }
                }
                else if (Variants.Contains(_sealed))
                {
                    var span = builder.TextFormatter.Format("h", style);
                    builder.Text(span, new Vector2(0.5, 2.5 + style.FontSize) - builder.CurrentTransform.Matrix.Inverse * span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.Upright);
                }

                if (Multiple > 1)
                {
                    builder.Line(new(2.6, -1.4), new(-0.2, -4.2), style);

                    var span = builder.TextFormatter.Format(Multiple.ToString(), style);
                    builder.Text(span, new Vector2(-0.2, -4.2) + new Vector2(-0.707, -0.707) * style.FontSize - builder.CurrentTransform.Matrix.Inverse * span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.Upright);
                }

                builder.ExtendPin(Pins["a"], style);
                _anchors.Draw(builder, this, style);
            }
        }
    }
}