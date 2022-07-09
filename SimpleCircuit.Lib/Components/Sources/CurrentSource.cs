using SimpleCircuit.Components.Pins;

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
            [Description("The label next to the source.")]
            public string Label { get; set; }

            [Description("The label on the other side of the source.")]
            public string Label2 { get; set; }

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
            public override void Reset()
            {
                base.Reset();
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(4, 0));
                        break;

                    case 0:
                    default:
                        SetPinOffset(0, new(-6, 0));
                        SetPinOffset(1, new(-6, 0));
                        break;
                }
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
                drawing.Line(new(-3, 0), new(3, 0), new("arrow") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });
                drawing.Text(Label, new(0, -8), new(0, -1));
                drawing.Text(Label2, new(0, 8), new(0, 1));
            }
            private void DrawEuropeanSource(SvgDrawing drawing)
            {
                drawing.Circle(new(), 4);
                drawing.Line(new(0, -4), new(0, 4));
                drawing.Text(Label, new(0, -6), new(0, -1));
                drawing.Text(Label2, new(0, 6), new(0, 1));
            }
        }
    }
}