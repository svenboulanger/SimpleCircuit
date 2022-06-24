using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A transmission line.
    /// </summary>
    [Drawable("TL", "A transmission line.", "Analog")]
    public class TransmissionLine : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private const double _width = 12.0;
            private const double _height = 3.0;
            private const double _rx = _height * 0.5;
            private const double _ry = _height;
            private const double _kx = 0.5522847498 * _rx;
            private const double _ky = 0.5522847498 * _ry;
            private const double _inner = _width - _rx;
            private double _length = _width;

            /// <summary>
            /// Draws the transmission line shape.
            /// </summary>
            /// <param name="builder">The path builder.</param>
            private static void DrawShape(PathBuilder builder)
            {
                builder
                    .MoveTo(new(-_inner, _height)).LineTo(new(_inner, _height))
                    .CurveTo(new(_inner + _kx, _height), new(_width, _ky), new(_width, 0))
                    .SmoothTo(new(_inner + _kx, -_height), new(_inner, -_height))
                    .LineTo(new(-_inner, -_height))
                    .CurveTo(new(-_inner - _kx, -_height), new(-_width, -_ky), new(-_width, 0))
                    .SmoothTo(new(-_inner - _kx, _height), new(-_inner, _height))
                    .SmoothTo(new(-_inner + _rx, _ky), new(-_inner + _rx, 0))
                    .SmoothTo(new(-_inner + _kx, -_height), new(-_inner, -_height));
            }

            [Description("The label in the transmission line.")]
            public string Label { get; set; }
            [Description("The length of the transmission line.")]
            public double Length
            {
                get => _length;
                set
                {
                    if (value < 8.0)
                        _length = 8.0;
                    else
                        _length = value;
                    UpdatePins();
                }
            }

            /// <inheritdoc />
            public override string Type => "transmissionline";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("left", "The left signal.", this, new(-_inner, 0), new(-1, 0)), "a", "l");
                Pins.Add(new FixedOrientedPin("leftground", "The left ground.", this, new(-_inner, _height), new(0, 1)), "ga", "gl");
                Pins.Add(new FixedOrientedPin("rightground", "The right ground.", this, new(_inner, _height), new(0, 1)), "gb", "gr");
                Pins.Add(new FixedOrientedPin("right", "The right signal.", this, new(_width, 0), new(1, 0)), "b", "r");
            }
            protected override void Draw(SvgDrawing drawing)
            {
                double offset = 0.5 * (Length - _width);

                // Wire
                if (Pins[0].Connections == 0)
                    drawing.ExtendPin(Pins[0]);
                if (Pins[1].Connections == 0)
                    drawing.ExtendPin(Pins[3]);

                // Transmission line
                drawing.Path(builder => DrawShape(builder.WithAbsoluteModifier(v =>
                {
                    if (v.X < 0)
                        return new(v.X - offset, v.Y);
                    else
                        return new(v.X + offset, v.Y);
                })));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(), new Vector2());
            }
            private void UpdatePins()
            {
                double x = -_inner - (Length - _width) / 2;
                SetPinOffset(0, new(x, 0));
                SetPinOffset(1, new(x, _height));

                x = _inner + (Length - _width) / 2;
                SetPinOffset(2, new(x, _height));
                SetPinOffset(3, new(x, 0));
            }
        }
    }
}