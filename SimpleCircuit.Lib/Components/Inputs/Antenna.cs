using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Inputs
{
    /// <summary>
    /// An antenna symbol.
    /// </summary>
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
                    new LabelAnchorPoint(new(5, -2), new(1, 0)),
                    new LabelAnchorPoint(new(-5, -2), new(-1, 0)));
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);

                switch (Variants.Select(_alt))
                {
                    case 0:
                        builder.Path(b =>
                        {
                            b.MoveTo(new(0, 0));
                            b.Line(new(0, -7));
                            b.MoveTo(new(0, 0));
                            b.LineTo(new(-5, -7));
                            b.LineTo(new(5, -7));
                            b.Close();
                        }, style);
                        break;

                    default:
                        builder.Path(b =>
                        {
                            b.MoveTo(new(0, 0));
                            b.Line(new(0, -7));
                            b.MoveTo(new(0, 0));
                            b.LineTo(new(-5, -7));
                            b.MoveTo(new(0, 0));
                            b.LineTo(new(5, -7));
                        }, style);
                        break;
                }

                builder.ExtendPins(Pins, style);
                _anchors.Draw(builder, this, style);
            }
        }
    }
}
