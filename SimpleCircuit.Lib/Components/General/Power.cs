using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A factory for power planes.
    /// </summary>
    [Drawable("POW", "A power plane.", "General", "vdd vcc vss vee")]
    public class PowerFactory : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(1);

            private const string _anchor = "anchor";

            /// <inheritdoc />
            public override string Type => "power";

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
                Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, 1)), "x", "p", "a");
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                if (Variants.Contains(_anchor))
                {
                    builder.Polyline([
                        new(-4, 4),
                        new(),
                        new(4, 4)
                    ], style);
                    _anchors[0] = new LabelAnchorPoint(new(0, -style.LineThickness * 0.5 - LabelMargin), new(0, -1));
                }
                else
                {
                    builder.Line(new Vector2(-5, 0), new Vector2(5, 0), style.AsLineThickness(1.0));
                    _anchors[0] = new LabelAnchorPoint(new(0, -0.5 - LabelMargin), new(0, -1));
                }

                builder.ExtendPins(Pins, style);
                _anchors.Draw(builder, this, style);
            }
        }
    }
}