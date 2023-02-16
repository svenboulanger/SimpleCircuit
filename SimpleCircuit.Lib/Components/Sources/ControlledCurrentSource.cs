using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A controlled current source.
    /// </summary>
    [Drawable(new[] { "G", "F" }, "A controlled current source.", new[] { "Sources" })]
    public class ControlledCurrentSource : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new(2);

            /// <inheritdoc />
            public override string Type => "ccs";

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
            public override bool Reset(IDiagnosticHandler diagnostics)
            {
                if (!base.Reset(diagnostics))
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

            /// <inheritdoc/>
            private void DrawAmericanSource(SvgDrawing drawing)
            {
                // Diamond
                drawing.Polygon(new Vector2[]
                {
                    new(-6, 0), new(0, 6), new(6, 0), new(0, -6)
                });

                // The circle with the arrow
                drawing.Arrow(new(-3, 0), new(3, 0), new("marker", "arrow"));

                // Depending on the orientation, let's anchor the text differently
                drawing.Text(Labels[0], new(0, -8), new(0, -1));
            }
            private void DrawEuropeanSource(SvgDrawing drawing)
            {
                drawing.Polygon(new Vector2[]
                {
                    new(-4, 0), new(0, 4), new(4, 0), new(0, -4)
                });
                drawing.Line(new(0, -4), new(0, 4));
                drawing.Text(Labels[0], new(0, -6), new(0, -1));
                drawing.Text(Labels[1], new(0, 6), new(0, 1));
            }
        }
    }
}