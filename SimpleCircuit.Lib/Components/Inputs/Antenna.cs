using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;

namespace SimpleCircuit.Components.Inputs
{
    [Drawable("ANT", "An antenna.", "Inputs")]
    public class Antenna : DrawableFactory
    {
        private const string _alt = "alt";

        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;

            /// <inheritdoc />
            public override string Type => "antenna";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("pin", "The pin of the antenna.", this, new(), new(0, 1)), "p", "pin", "a");
                _anchors = new(
                    new LabelAnchorPoint(new(5, -5), new(1, 0)),
                    new LabelAnchorPoint(new(-5, -5), new(-1, 0)));
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.Modify(Style);
                if (Variants.Contains(_alt))
                {
                    builder.Path(b =>
                    {
                        b.MoveTo(new(0, 0));
                        b.Line(new(0, -10));
                        b.MoveTo(new(0, -3));
                        b.LineTo(new(-5, -10));
                        b.LineTo(new(5, -10));
                        b.LineTo(new(0, -3));
                    }, style);
                }
                else
                {
                    builder.Path(b =>
                    {
                        b.MoveTo(new(0, 0));
                        b.Line(new(0, -10));
                        b.MoveTo(new(0, -3));
                        b.LineTo(new(-5, -10));
                        b.MoveTo(new(0, -3));
                        b.LineTo(new(5, -10));
                    }, style);
                }

                _anchors.Draw(builder, this, style);
            }
        }
    }
}
