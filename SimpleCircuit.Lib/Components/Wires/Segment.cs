using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

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
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());
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
                builder.ExtendPins(Pins, Appearance, 4);

                _anchors[0] = new LabelAnchorPoint(new(0, -1), new(0, -1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(0, 1), new(0, 1), Appearance);
                switch (Variants.Select(_underground, _air, _tube, _inwall, _onwall))
                {
                    case 0: DrawUnderground(builder); break;
                    case 1: DrawAir(builder); break;
                    case 2: DrawTube(builder); break;
                    case 3: DrawInWall(builder); break;
                    case 4: DrawOnWall(builder); break;
                }

                _anchors.Draw(builder, this);
            }
            private void DrawUnderground(IGraphicsBuilder builder)
            {
                builder.Path(b => b
                    .MoveTo(new(-4, -5))
                    .Line(new(8, 0))
                    .MoveTo(new(-2.5, -3.5))
                    .Line(new(5, 0))
                    .MoveTo(new(-1, -2))
                    .Line(new(2, 0)), Appearance);
                if (_anchors[0].Location.Y > -6)
                    _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1), Appearance);
                if (_anchors[1].Location.Y < 1)
                    _anchors[1] = new LabelAnchorPoint(new(0, 1), new(0, 1), Appearance);
            }
            private void DrawAir(IGraphicsBuilder builder)
            {
                builder.Circle(new(), 2, Appearance);
                if (_anchors[0].Location.Y > -3)
                    _anchors[0] = new LabelAnchorPoint(new(0, -3), new(0, -1), Appearance);
                if (_anchors[1].Location.Y < 3)
                    _anchors[1] = new LabelAnchorPoint(new(0, 3), new(0, 1), Appearance);
            }
            private void DrawTube(IGraphicsBuilder builder)
            {
                builder.Circle(new(0, -3.5), 1.5, Appearance);
                if (_anchors[0].Location.Y > -6)
                    _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1), Appearance);
                if (_anchors[1].Location.Y < 1)
                    _anchors[1] = new LabelAnchorPoint(new(0, 1), new(0, 1), Appearance);

                if (Multiple > 1)
                {
                    builder.Line(new(0, -3.5), new(2.1, -5.6), Appearance);
                    builder.Text(Multiple.ToString(), new(2.5, -5.1), new(1, -1), Appearance);
                    _anchors[0] = new LabelAnchorPoint(new(0, -11), new(0, -1), Appearance);
                }
            }
            private void DrawInWall(IGraphicsBuilder builder)
            {
                builder.Polyline([
                    new(-3, -2),
                    new(-3, -5),
                    new(3, -5),
                    new(3, -2)
                ], Appearance);
                builder.Line(new(0, -2), new(0, -5), Appearance);
                if (_anchors[0].Location.Y > -6)
                    _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1), Appearance);
                if (_anchors[1].Location.Y < 1)
                    _anchors[1] = new LabelAnchorPoint(new(0, 1), new(0, 1), Appearance);
            }
            private void DrawOnWall(IGraphicsBuilder builder)
            {
                builder.Polyline([
                    new(-3, 5), 
                    new(-3, 2), 
                    new(3, 2), 
                    new(3, 5)
                ], Appearance);
                builder.Line(new(0, 5), new(0, 2), Appearance);
                if (_anchors[0].Location.Y > -1)
                    _anchors[0] = new LabelAnchorPoint(new(0, -1), new(0, -1), Appearance);
                if (_anchors[1].Location.Y < 6)
                    _anchors[1] = new LabelAnchorPoint(new(0, 6), new(0, 1), Appearance);
            }
        }
    }
}