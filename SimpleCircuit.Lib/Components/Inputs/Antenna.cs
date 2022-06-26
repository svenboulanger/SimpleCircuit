using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Inputs
{
    [Drawable("ANT", "An antenna.", "Inputs")]
    public class Antenna : DrawableFactory
    {
        private const string _alt = "alt";

        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("Adds a label next to the antenna.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "antenna";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("pin", "The pin of the antenna.", this, new(), new(0, 1)), "p", "pin", "a");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (Variants.Contains(_alt))
                {
                    drawing.Path(b =>
                    {
                        b.MoveTo(0, 0);
                        b.Line(0, -10);
                        b.MoveTo(0, -3);
                        b.LineTo(-5, -10);
                        b.LineTo(5, -10);
                        b.LineTo(0, -3);
                    });
                }
                else
                {
                    drawing.Path(b =>
                    {
                        b.MoveTo(0, 0);
                        b.Line(0, -10);
                        b.MoveTo(0, -3);
                        b.LineTo(-5, -10);
                        b.MoveTo(0, -3);
                        b.LineTo(5, -10);
                    });
                }

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(5, -5), new(1, 0));
            }
        }
    }
}
