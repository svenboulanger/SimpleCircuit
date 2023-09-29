using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire segment that has a bit more information.
    /// </summary>
    [Drawable("SEG", "A wire segment.", "Wires")]
    public class Segment : DrawableFactory
    {
        private const string _underground = "underground";
        private const string _air = "air";
        private const string _tube = "tube";
        private const string _inwall = "inwall";
        private const string _onwall = "onwall";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private double _textY = 0;

            /// <inheritdoc />
            public override string Type => "segment";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

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

                _textY = 0.0;
                switch (Variants.Select(_underground, _air, _tube, _inwall, _onwall))
                {
                    case 0: DrawUnderground(drawing); break;
                    case 1: DrawAir(drawing); break;
                    case 2: DrawTube(drawing); break;
                    case 3: DrawInWall(drawing); break;
                    case 4: DrawOnWall(drawing); break;
                }

                Labels.SetDefaultPin(0, location: new(0, _textY - 2), expand: new(0, -1));
                Labels.Draw(drawing);
            }
            private void DrawUnderground(SvgDrawing drawing)
            {
                drawing.Path(b => b.MoveTo(-4, -5).Line(8, 0).MoveTo(-2.5, -3.5).Line(5, 0).MoveTo(-1, -2).Line(2, 0));
                _textY = Math.Min(_textY, -5);
            }
            private void DrawAir(SvgDrawing drawing)
            {
                drawing.Circle(new(), 2);
                _textY = Math.Min(_textY, -2);
            }
            private void DrawTube(SvgDrawing drawing)
            {
                drawing.Circle(new(0, -3.5), 1.5);
                _textY = Math.Min(_textY, -5);
            }
            private void DrawInWall(SvgDrawing drawing)
            {
                drawing.Polyline(new Vector2[] { new(-3, -2), new(-3, -5), new(3, -5), new(3, -2) });
                drawing.Line(new(0, -2), new(0, -5));
                _textY = Math.Min(_textY, -5);
            }
            private void DrawOnWall(SvgDrawing drawing)
            {
                drawing.Polyline(new Vector2[] { new(-3, 5), new(-3, 2), new(3, 2), new(3, 5) });
                drawing.Line(new(0, 5), new(0, 2));
                _textY = Math.Min(_textY, 0);
            }
        }
    }
}