using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

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
            private readonly CustomLabelAnchorPoints _anchors;

            private const string _anchor = "anchor";

            /// <inheritdoc />
            public override string Type => "power";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, 1)), "x", "p", "a");
                _anchors = new(new LabelAnchorPoint(new(0, -1.5), new(0, -1)));
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.Modify(Style);
                if (Variants.Contains(_anchor))
                {
                    builder.Polyline([
                        new(-4, 4),
                        new(),
                        new(4, 4)
                    ], style);
                }
                else
                {
                    builder.Line(new Vector2(-5, 0), new Vector2(5, 0), style.AsLineThickness(1.0));
                }

                builder.ExtendPins(Pins, style);

                _anchors.Draw(builder, this, style);
            }
        }
    }
}