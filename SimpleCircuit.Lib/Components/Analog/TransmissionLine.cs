using SimpleCircuit.Circuits.Contexts;
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
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

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
            private void DrawShape(PathBuilder builder)
            {
                double offset = 0.5 * (Length - _width);
                double inner = _inner + offset;
                double width = _width + offset;
                builder
                    .MoveTo(new(-inner, _height)).LineTo(new(inner, _height))
                    .CurveTo(new(inner + _kx, _height), new(width, _ky), new(width, 0))
                    .SmoothTo(new(inner + _kx, -_height), new(inner, -_height))
                    .LineTo(new(-inner, -_height))
                    .CurveTo(new(-inner - _kx, -_height), new(-width, -_ky), new(-width, 0))
                    .SmoothTo(new(-inner - _kx, _height), new(-inner, _height))
                    .SmoothTo(new(-inner + _rx, _ky), new(-inner + _rx, 0))
                    .SmoothTo(new(-inner + _kx, -_height), new(-inner, -_height));
            }

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            [Description("The length of the transmission line.")]
            public double Length
            {
                get => _length;
                set
                {
                    _length = value;
                    if (_length < 8.0)
                        _length = 8.0;
                }
            }

            /// <inheritdoc />
            public override string Type => "transmissionline";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("left", "The left signal.", this, new(-_inner, 0), new(-1, 0)), "a", "l");
                Pins.Add(new FixedOrientedPin("leftground", "The left ground.", this, new(-_inner, _height), new(0, 1)), "ga", "gl");
                Pins.Add(new FixedOrientedPin("rightground", "The right ground.", this, new(_inner, _height), new(0, 1)), "gb", "gr");
                Pins.Add(new FixedOrientedPin("right", "The right signal.", this, new(_width, 0), new(1, 0)), "b", "r");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                double x = -_inner - (Length - _width) / 2;
                SetPinOffset(0, new(x, 0));
                SetPinOffset(1, new(x, _height));

                x = _inner + (Length - _width) / 2;
                SetPinOffset(2, new(x, _height));
                SetPinOffset(3, new(_width + (Length - _width) / 2, 0));
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                // Wire
                drawing.ExtendPins(Pins, 2, "a", "b");

                // Transmission line
                drawing.Path(DrawShape);

                // Label
                Labels.SetDefaultPin(-1, location: new(), expand: new());
                Labels.SetDefaultPin(1, location: new(0, -_height - 1), expand: new(0, -1));
                Labels.SetDefaultPin(2, location: new(0, _height + 1), expand: new(0, 1));
                Labels.Draw(drawing);
            }
        }
    }
}