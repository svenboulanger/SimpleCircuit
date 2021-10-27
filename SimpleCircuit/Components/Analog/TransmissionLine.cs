using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A transmission line.
    /// </summary>
    /// <seealso cref="TransformingComponent"/>
    /// <seealso cref="ILabeled"/>
    [SimpleKey("TL", "Transmission line", Category = "Analog")]
    public class TransmissionLine : TransformingComponent, ILabeled
    {
        private const double _width = 16.0;
        private const double _height = 4.0;
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

        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransmissionLine"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        public TransmissionLine(string name)
            : base(name)
        {
            Pins.Add(new[] { "a", "l" }, "The left signal.", new Vector2(-_inner, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "ga", "gl" }, "The left ground.", new Vector2(-_inner, _height), new Vector2(0, 1));
            Pins.Add(new[] { "gb", "gr" }, "The right ground.", new Vector2(_inner, _height), new Vector2(0, 1));
            Pins.Add(new[] { "b", "r" }, "The right signal.", new Vector2(_width, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Line(new Vector2(-_width, 0), new Vector2(-_inner, 0));
            drawing.OpenBezier(_shape);

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
