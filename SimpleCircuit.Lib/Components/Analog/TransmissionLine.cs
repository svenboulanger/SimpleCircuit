using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;
using System.Linq;

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
            private const double _width = 8.0;
            private const double _height = 3.0;
            private const double _rx = _height * 0.5;
            private const double _ry = _height;
            private const double _kx = 0.5522847498 * _rx;
            private const double _ky = 0.5522847498 * _ry;
            private const double _inner = _width - _rx;
            private readonly static Action<PathBuilder> _shape = builder => builder
                .MoveTo(new(-_inner, _height)).LineTo(new(_inner, _height))
                .CurveTo(new(_inner + _kx, _height), new(_width, _ky), new(_width, 0))
                .SmoothTo(new(_inner + _kx, -_height), new(_inner, -_height))
                .LineTo(new(-_inner, -_height))
                .CurveTo(new(-_inner - _kx, -_height), new(-_width, -_ky), new(-_width, 0))
                .SmoothTo(new(-_inner - _kx, _height), new(-_inner, _height))
                .SmoothTo(new(-_inner + _rx, _ky), new(-_inner + _rx, 0))
                .SmoothTo(new(-_inner + _kx, -_height), new(-_inner, -_height));

            [Description("The label in the transmission line.")]
            public string Label { get; set; }
            [Description("The length of the transmission line.")]
            public double Length { get; set; }

            /// <inheritdoc />
            public override string Type => "transmissionline";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("left", "The left signal.", this, new(-_inner, 0), new(-1, 0)), "a", "l");
                Pins.Add(new FixedOrientedPin("leftground", "The left ground.", this, new(-_inner, _height), new(0, 1)), "ga", "gl");
                Pins.Add(new FixedOrientedPin("rightground", "The right ground.", this, new(_inner, _height), new(0, 1)), "gb", "gr");
                Pins.Add(new FixedOrientedPin("right", "The right signal.", this, new(_width, 0), new(1, 0)), "b", "r");

                PinUpdate = Variant.Do(UpdatePins);
                DrawingVariants = Variant.Do(DrawTransmissionLine);
            }
            private void DrawTransmissionLine(SvgDrawing drawing)
            {
                double offset = 0.5 * (Length - _width);

                // Wire
                drawing.Line(new(-_width - offset, 0), new(-_inner - offset, 0), new("wire"));

                // Transmission line
                /*drawing.OpenBezier(_shape.Select(v =>
                {
                    if (v.X < 0)
                        return new Vector2(v.X - offset, v.Y);
                    else
                        return new Vector2(v.X + offset, v.Y);
                }));
                */
                drawing.Path(builder => _shape(builder.WithAbsoluteModifier(v =>
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
                Length = Math.Max(8, Length);
                ((FixedOrientedPin)Pins[0]).Offset = new(-_inner - (Length - _width) / 2, 0);
                ((FixedOrientedPin)Pins[1]).Offset = new(-_inner - (Length - _width) / 2, _height);
                ((FixedOrientedPin)Pins[2]).Offset = new(_inner + (Length - _width) / 2, _height);
                ((FixedOrientedPin)Pins[3]).Offset = new(_width + (Length - _width) / 2, 0);
            }
        }
    }
}