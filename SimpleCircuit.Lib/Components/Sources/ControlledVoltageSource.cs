using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A controlled voltage source.
    /// </summary>
    [Drawable(new[] { "E", "H" }, "A controlled voltage source.", new[] { "Sources" })]
    public class ControlledVoltageSource : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "cvs";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-6, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(6, 0), new(1, 0)), "p", "pos", "a");
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

            /// <inheritdoc />
            private void DrawAmericanSource(SvgDrawing drawing)
            {
                // Diamond
                drawing.Polygon(new Vector2[]
                {
                    new(-6, 0), new(0, 6), new(6, 0), new(0, -6)
                });

                // Plus and minus
                drawing.Line(new(-3, -1), new(-3, 1), new("minus"));
                drawing.Path(b => b.MoveTo(3, -1).Line(0, 2).MoveTo(2, 0).Line(2, 0), new("plus"));

                // Label
                Labels.SetDefaultPin(-1, location: new(0, -8), expand: new(0, -1));
                Labels.SetDefaultPin(1, location: new(0, 8), expand: new(0, 1));
                Labels.Draw(drawing);
            }
            private void DrawEuropeanSource(SvgDrawing drawing)
            {
                drawing.Polygon(new Vector2[]
                {
                    new(-4, 0), new(0, 4), new(4, 0), new(0, -4)
                });
                drawing.Line(new(-4, 0), new(4, 0));

                Labels.SetDefaultPin(-1, location: new(0, -6), expand: new(0, -1));
                Labels.SetDefaultPin(1, location: new(0, 6), expand: new(0, 1));
                Labels.Draw(drawing);
            }
        }
    }
}