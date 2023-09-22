using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A light.
    /// </summary>
    [Drawable("LIGHT", "A light point.", "Outputs")]
    public class Light : DrawableFactory
    {
        private const string _direction = "direction";
        private const string _diverging = "diverging";
        private const string _projector = "projector";
        private const string _emergency = "emergency";
        private const string _wall = "wall";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            private static readonly double _sqrt2 = Math.Sqrt(2) * 4;

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.AREI;

            /// <inheritdoc />
            public override string Type => "light";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "b", "n", "neg");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                if (Variants.Contains(Options.Arei))
                {
                    SetPinOffset(0, new());
                    SetPinOffset(1, new());
                }
                else
                {
                    SetPinOffset(0, new(-4, 0));
                    SetPinOffset(1, new(4, 0));
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.Cross(new(), _sqrt2);

                if (!Variants.Contains(Options.Arei))
                    drawing.Circle(new Vector2(), 4);
                else
                {
                    if (Variants.Contains(_wall))
                        DrawWall(drawing);
                    if (Variants.Contains(_projector))
                        DrawProjector(drawing);
                    if (Variants.Contains(_direction))
                        DrawDirectional(drawing, Variants.Contains(_diverging));
                    if (Variants.Contains(_emergency))
                        DrawEmergency(drawing);
                }

                // Label
                drawing.Label(Labels, 0, new Vector2(0, -5), new Vector2(0, -1));
            }

            private void DrawWall(SvgDrawing drawing)
            {
                drawing.Line(new Vector2(-3, 5), new Vector2(3, 5));
            }
            private void DrawProjector(SvgDrawing drawing)
            {
                drawing.Arc(new(), -Math.PI * 0.95, -Math.PI * 0.05, 6, new("projector"), 1);
            }
            private void DrawDirectional(SvgDrawing drawing, bool diverging)
            {
                var options = new GraphicOptions("direction");
                if (diverging)
                {
                    drawing.Arrow(new(-2, 6), new(-6, 12), options);
                    drawing.Arrow(new(2, 6), new(6, 12), options);
                }
                else
                {
                    drawing.Arrow(new(-2, 6), new(-2, 12), options);
                    drawing.Arrow(new(2, 6), new(2, 12), options);
                }
            }
            private void DrawEmergency(SvgDrawing drawing)
            {
                drawing.Circle(new(), 1.5, new("dot"));
            }
        }
    }
}