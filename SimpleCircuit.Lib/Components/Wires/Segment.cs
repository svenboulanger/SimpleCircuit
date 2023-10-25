using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;

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

        private class Instance : ScaledOrientedDrawable, ILabeled
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

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            [Description("The number of tubes.")]
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
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 4);

                _anchors[0] = new LabelAnchorPoint(new(0, -1), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 1), new(0, 1));
                switch (Variants.Select(_underground, _air, _tube, _inwall, _onwall))
                {
                    case 0: DrawUnderground(drawing); break;
                    case 1: DrawAir(drawing); break;
                    case 2: DrawTube(drawing); break;
                    case 3: DrawInWall(drawing); break;
                    case 4: DrawOnWall(drawing); break;
                }

                _anchors.Draw(drawing, this);
            }
            private void DrawUnderground(SvgDrawing drawing)
            {
                drawing.Path(b => b.MoveTo(-4, -5).Line(8, 0).MoveTo(-2.5, -3.5).Line(5, 0).MoveTo(-1, -2).Line(2, 0));
                if (_anchors[0].Location.Y > -6)
                    _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1));
                if (_anchors[1].Location.Y < 1)
                    _anchors[1] = new LabelAnchorPoint(new(0, 1), new(0, 1));
            }
            private void DrawAir(SvgDrawing drawing)
            {
                drawing.Circle(new(), 2);
                if (_anchors[0].Location.Y > -3)
                    _anchors[0] = new LabelAnchorPoint(new(0, -3), new(0, -1));
                if (_anchors[1].Location.Y < 3)
                    _anchors[1] = new LabelAnchorPoint(new(0, 3), new(0, 1));
            }
            private void DrawTube(SvgDrawing drawing)
            {
                drawing.Circle(new(0, -3.5), 1.5);
                if (_anchors[0].Location.Y > -6)
                    _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1));
                if (_anchors[1].Location.Y < 1)
                    _anchors[1] = new LabelAnchorPoint(new(0, 1), new(0, 1));

                if (Multiple > 1)
                {
                    drawing.Line(new(0, -3.5), new(2.1, -5.6));
                    drawing.Text(Multiple.ToString(), new(2.5, -5.1), new(1, -1));
                    _anchors[0] = new LabelAnchorPoint(new(0, -11), new(0, -1));
                }
            }
            private void DrawInWall(SvgDrawing drawing)
            {
                drawing.Polyline(new Vector2[] { new(-3, -2), new(-3, -5), new(3, -5), new(3, -2) });
                drawing.Line(new(0, -2), new(0, -5));
                if (_anchors[0].Location.Y > -6)
                    _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1));
                if (_anchors[1].Location.Y < 1)
                    _anchors[1] = new LabelAnchorPoint(new(0, 1), new(0, 1));
            }
            private void DrawOnWall(SvgDrawing drawing)
            {
                drawing.Polyline(new Vector2[] { new(-3, 5), new(-3, 2), new(3, 2), new(3, 5) });
                drawing.Line(new(0, 5), new(0, 2));
                if (_anchors[0].Location.Y > -1)
                    _anchors[0] = new LabelAnchorPoint(new(0, -1), new(0, -1));
                if (_anchors[1].Location.Y < 6)
                    _anchors[1] = new LabelAnchorPoint(new(0, 6), new(0, 1));
            }
        }
    }
}