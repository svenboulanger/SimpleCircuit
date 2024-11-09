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

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private static readonly CustomLabelAnchorPoints _anchors = new(new LabelAnchorPoint(new(0, -1.5), new(0, -1)));

            private const string _anchor = "anchor";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

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
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins);

                if (Variants.Contains(_anchor))
                    builder.Polyline([
                        new(-4, 4),
                        new(),
                        new(4, 4)
                    ], new("anchor"));
                else
                {
                    builder.RequiredCSS.Add(".plane { stroke-width: 1pt; }");
                    builder.Line(new Vector2(-5, 0), new Vector2(5, 0), new("plane"));
                }
                _anchors.Draw(builder, this);
            }
        }
    }
}