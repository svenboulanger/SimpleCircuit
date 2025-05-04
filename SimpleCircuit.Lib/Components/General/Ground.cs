using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A ground terminal.
    /// </summary>
    [Drawable("GND", "A common ground symbol.", "General", "earth chassis vss vee")]
    [Drawable("SGND", "A signal ground symbol.", "General", "earth chassis vss vee")]
    public class Ground : DrawableFactory
    {
        private const string _earth = "earth";
        private const string _chassis = "chassis";
        private const string _signal = "signal";
        private const string _noiseless = "noiseless";
        private const string _protective = "protective";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var device = new Instance(name);
            if (key == "SGND")
                device.Variants.Add("signal");
            return device;
        }

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;
            /// <inheritdoc />
            public override string Type => "ground";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("p", "The one and only pin.", this, new(0, 0), new(0, -1)), "a", "p");
                _anchors = new(
                    new LabelAnchorPoint(new(-6, 0), new(-1, 0), Appearance),
                    new LabelAnchorPoint(new(6, 0), new(1, 0), Appearance));
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                _anchors[0] = new LabelAnchorPoint(new(-6, 0), new(-1, 0), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(6, 0), new(1, 0), Appearance);
                switch (Variants.Select(_earth, _chassis, _signal))
                {
                    case 0:
                    case 1: DrawEarth(builder); break;
                    case 2: DrawSignalGround(builder); break;
                    default: DrawGround(builder); break;
                }

                _anchors.Draw(builder, this);
            }
            private void DrawGround(IGraphicsBuilder drawing)
            {
                drawing.ExtendPins(Pins, Appearance, Variants.Contains(_protective) ? 9 : 3);

                if (Variants.Contains(_noiseless))
                {
                    drawing.ExtendPins(Pins, Appearance, 6);
                    drawing.Path(b => b.MoveTo(new(-8, 4)).ArcTo(8, 8, 0, true, true, new(8, 4)), new("shield"));
                    if (_anchors[0].Location.X > -9)
                        _anchors[0] = new LabelAnchorPoint(new(-9, 0), new(-1, 0), Appearance);
                    if (_anchors[0].Location.X < 9)
                        _anchors[1] = new LabelAnchorPoint(new(9, 0), new(1, 0), Appearance);
                }
                if (Variants.Contains(_protective))
                {
                    drawing.ExtendPins(Pins, Appearance, 7.5);
                    drawing.Circle(new(0, -1), 6.5, new("shield"));
                    if (_anchors[0].Location.X > -7.5) 
                        _anchors[0] = new LabelAnchorPoint(new(-7.5, 0), new(-1, 0), Appearance);
                    if (_anchors[1].Location.X < 7.5)
                        _anchors[1] = new LabelAnchorPoint(new(7.5, 0), new(1, 0), Appearance);
                }
                else
                {
                    drawing.ExtendPins(Pins, Appearance, 3);
                }
                drawing.Path(b => b
                    .MoveTo(new(-5, 0))
                    .LineTo(new(5, 0))
                    .MoveTo(new(-3, 2))
                    .LineTo(new(3, 2))
                    .MoveTo(new(-1, 4))
                    .LineTo(new(1, 4)));
            }
            private void DrawEarth(IGraphicsBuilder drawing)
            {
                drawing.ExtendPins(Pins, Appearance, 3);

                // Ground segments
                drawing.Path(b => b
                    .MoveTo(new(-5, 0))
                    .LineTo(new(5, 0))
                    .MoveTo(new(-5, 0))
                    .Line(new(-2, 4))
                    .MoveTo(new(0, 0))
                    .Line(new(-2, 4))
                    .MoveTo(new(5, 0))
                    .Line(new(-2, 4)));

                if (_anchors[0].Location.X > -7)
                    _anchors[0] = new LabelAnchorPoint(new(-7, 0), new(-1, 0), Appearance);
            }
            private void DrawSignalGround(IGraphicsBuilder drawing)
            {
                drawing.ExtendPins(Pins, Appearance, 3);

                // Ground
                drawing.Polygon([
                    new(-5, 0),
                    new(5, 0),
                    new(0, 4)
                ]);
            }
        }
    }
}
