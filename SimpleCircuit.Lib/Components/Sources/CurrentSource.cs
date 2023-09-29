using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Markers;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    [Drawable("I", "A current source.", "Sources")]
    public class CurrentSource : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "cs";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The current end point.", this, new(-6, 0), new(-1, 0)), "p", "b");
                Pins.Add(new FixedOrientedPin("negative", "The current starting point.", this, new(6, 0), new(1, 0)), "n", "a");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(4, 0));
                        break;

                    case 0:
                    default:
                        SetPinOffset(0, new(-6, 0));
                        SetPinOffset(1, new(6, 0));
                        break;
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        DrawEuropeanSource(drawing);
                        break;

                    case 0:
                    default:
                        DrawAmericanSource(drawing);
                        break;
                }
            }

            private void DrawAmericanSource(SvgDrawing drawing)
            {
                drawing.Circle(new(0, 0), 6);
                switch (Variants.Select("arrow", "ac"))
                {
                    case 0:
                        drawing.Line(new(-3, 0), new(3, 0), new("arrow"));
                        var marker = new Arrow(new(-3, 0), new(-1, 0));
                        marker.Draw(drawing);
                        marker.Location = new(3, 0);
                        marker.Orientation = new(1, 0);
                        marker.Draw(drawing);
                        break;

                    default:
                        drawing.Arrow(new(-3, 0), new(3, 0), new("marker", "arrow"));
                        break;
                }

                Labels.SetDefaultPin(0, location: new(0, -8), expand: new(0, -1));
                Labels.SetDefaultPin(1, location: new(0, 8), expand: new(0, 1));
                Labels.Draw(drawing);
            }
            private void DrawEuropeanSource(SvgDrawing drawing)
            {
                drawing.Circle(new(), 4);
                drawing.Line(new(0, -4), new(0, 4));

                Labels.SetDefaultPin(0, location: new(0, -8), expand: new(0, -1));
                Labels.SetDefaultPin(1, location: new(0, 8), expand: new(0, 1));
                Labels.Draw(drawing);
            }
        }
    }
}