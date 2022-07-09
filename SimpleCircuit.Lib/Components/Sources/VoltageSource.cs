using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    [Drawable("V", "A voltage source.", "Sources")]
    public class VoltageSource : DrawableFactory
    {
        private const string _ac = "ac";
        private const string _pulse = "pulse";
        private const string _tri = "tri";

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
            public override string Type => "vs";

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
                        SetPinOffset(1, new(6, 0));
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
                // Circle
                drawing.Circle(new(0, 0), 6);

                // Waveform / inner graphic
                switch (Variants.Select(_ac, _pulse, _tri))
                {
                    case 0:
                        drawing.AC(vertical: true);
                        break;

                    case 1:
                        drawing.Polyline(new Vector2[]
                        {
                            new(0, -3), new(3, -3), new(3, 0), new(-3, 0), new(-3, 3), new(0, 3)
                        });
                        break;

                    case 2:
                        drawing.Polyline(new Vector2[]
                        {
                            new(0, -3), new(1.5, -1.5), new(-1.5, 1.5), new(0, 3)
                        });
                        break;

                    default:
                        drawing.Signs(new(3, 0), new(-3, 0), vertical: true);
                        break;
                }

                // Label
                drawing.Text(Label, new(0, -8), new(0, -1));
                drawing.Text(Label2, new(0, 8), new(0, 1));
            }

            private void DrawEuropeanSource(SvgDrawing drawing)
            {
                drawing.Circle(new(0, 0), 4);
                drawing.Line(new(-4, 0), new(4, 0));
                drawing.Text(Label, new(0, -6), new(0, -1));
                drawing.Text(Label2, new(0, 6), new(0, 1));
            }
        }
    }
}