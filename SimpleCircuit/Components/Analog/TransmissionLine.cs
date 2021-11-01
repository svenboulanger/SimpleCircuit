using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A transmission line.
    /// </summary>
    [SimpleKey("TL", "A transmission line.", Category = "Analog")]
    public class TransmissionLine : ScaledOrientedDrawable, ILabeled
    {
        private const double _width = 8.0;
        private const double _height = 3.0;
        private const double _rx = _height * 0.5;
        private const double _ry = _height;
        private const double _kx = 0.5522847498 * _rx;
        private const double _ky = 0.5522847498 * _ry;
        private const double _inner = _width - _rx;
        private readonly static Vector2[] _shape = new[]
        {
            new Vector2(-_inner, _height),
            new Vector2(-_inner, _height), new Vector2(_inner, _height), new Vector2(_inner, _height),
            new Vector2(_inner + _kx, _height), new Vector2(_width, _ky), new Vector2(_width, 0),
            new Vector2(_width, -_ky), new Vector2(_inner + _kx, -_height), new Vector2(_inner, -_height),
            new Vector2(_inner, -_height), new Vector2(-_inner, -_height), new Vector2(-_inner, -_height),
            new Vector2(-_inner - _kx, -_height), new Vector2(-_width, -_ky), new Vector2(-_width, 0),
            new Vector2(-_width, _ky), new Vector2(-_inner - _kx, _height), new Vector2(-_inner, _height),
            new Vector2(-_inner + _kx, _height), new Vector2(-_inner + _rx, _ky), new Vector2(-_inner + _rx, 0),
            new Vector2(-_inner + _rx, -_ky), new Vector2(-_inner + _kx, -_height), new Vector2(-_inner, -_height)
        };

        private double _length = 8;

        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the length of the transmission line.
        /// </summary>
        public double Length
        {
            get => _length;
            set
            {
                if (value < _width)
                    _length = _width;
                else
                    _length = value;
                ((FixedOrientedPin)Pins[0]).Offset = new(-_inner - (_length - _width) / 2, 0);
                ((FixedOrientedPin)Pins[1]).Offset = new(-_inner - (_length - _width) / 2, _height);
                ((FixedOrientedPin)Pins[2]).Offset = new(_inner + (_length - _width) / 2, _height);
                ((FixedOrientedPin)Pins[3]).Offset = new(_width + (_length - _width) / 2, 0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransmissionLine"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        public TransmissionLine(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("left", "The left signal.", this, new(-_inner, 0), new(-1, 0)), "a", "l");
            Pins.Add(new FixedOrientedPin("leftground", "The left ground.", this, new(-_inner, _height), new(0, 1)), "ga", "gl");
            Pins.Add(new FixedOrientedPin("rightground", "The right ground.", this, new(_inner, _height), new(0, 1)), "gb", "gr");
            Pins.Add(new FixedOrientedPin("right", "The right signal.", this, new(_width, 0), new(1, 0)), "b", "r");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            double offset = 0.5 * (_length - _width);
            drawing.Line(new Vector2(-_width - offset, 0), new Vector2(-_inner - offset, 0));
            drawing.OpenBezier(_shape.Select(v =>
            {
                if (v.X < 0)
                    return new Vector2(v.X - offset, v.Y);
                else
                    return new Vector2(v.X + offset, v.Y);
            }));

            if (!string.IsNullOrWhiteSpace(Label))
            {
                drawing.Text(Label, new Vector2(), new Vector2());
            }
        }

        /// <summary>
        /// Converts the component to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => $"Transmission line {Name}";
    }
}
