using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An impedance/admittance.
    /// </summary>
    [Drawable("Z", "An impedance.", "Analog")]
    [Drawable("Y", "An admittance.", "Analog")]
    public class Impedance : DrawableFactory
    {
        private const string _programmable = "programmable";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(key == "Y" ? "admittance" : "impedance", name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private double _width = 6, _length = 12;

            /// <inheritdoc />
            public Labels Labels { get; } = new(2);

            /// <inheritdoc />
            public override string Type { get; }

            [Description("The length of the element")]
            public double Length
            {
                get => _length;
                set
                {
                    _length = value;
                    if (_length < 6)
                        _length = 6;
                }
            }

            [Description("The width of the element")]
            public double Width
            {
                get => _width;
                set
                {
                    _width = value;
                    if (_width < 4)
                        _width = 4;
                }
            }

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="name">The name.</param>
            public Instance(string type, string name)
                : base(name)
            {
                Type = type;
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "neg", "b");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // The rectangle
                double w = Width * 0.5;
                drawing.Rectangle(-Length * 0.5, -w, Length, Width);

                if (Variants.Contains(_programmable))
                {
                    drawing.Arrow(new(-5, w + 1), new(6, -w - 4));
                    Labels.Draw(drawing, 0, new(0, -w - 4), new(0, -1));
                }
                else
                    Labels.Draw(drawing, 0, new(0, -w - 1), new(0, -1));

                // The label
                Labels.Draw(drawing, 1, new(0, w + 1), new(0, 1));
            }
        }
    }
}
