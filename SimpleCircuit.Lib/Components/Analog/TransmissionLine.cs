using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;

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

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(3);
            private double _width, _length, _rx;

            [Description("The margin used for the center label if the size of the transmission line is calculated from the center label.")]
            [Alias("m")]
            public Margins Margin { get; set; } = new(2, 2, 2, 2);

            [Description("The length of the transmission line. If 0, the length is based on the center label and the minimum length.")]
            [Alias("l")]
            public double Length { get; set; }

            [Description("The minimum length of the transmission line. Default is 24.")]
            public double MinLength { get; set; } = 24.0;

            [Description("The width of the transmission line. If 0, the width is based on the center label and the minimum width.")]
            [Alias("w")]
            public double Width { get; set; }

            [Description("The minimum width of the transmission line. Default is 6.")]
            public double MinWidth { get; set; } = 6.0;

            /// <summary>
            /// Draws the transmission line shape.
            /// </summary>
            /// <param name="builder">The path builder.</param>
            private void DrawShape(IPathBuilder builder)
            {
                builder.MoveTo(new(-_length * 0.5 + _rx, _width * 0.5));
                builder.LineTo(new(_length * 0.5 - _rx, _width * 0.5));
                builder.ArcTo(_rx, _width * 0.5, 0.0, false, false, new(_length * 0.5 - _rx, -_width * 0.5));
                builder.LineTo(new(-_length * 0.5 + _rx, -_width * 0.5));
                builder.ArcTo(_rx, _width * 0.5, 0.0, false, false, new(-_length * 0.5 + _rx, _width * 0.5));
                builder.ArcTo(_rx, _width * 0.5, 0.0, false, false, new(-_length * 0.5 + _rx, -_width * 0.5));
                // builder.ArcTo(_rx, _width * 0.5, 0.0, false, true, new(-_length * 0.5 + _rx, -_width * 0.5));

                //double offset = 0.5 * (Length - _width);
                //double inner = _inner + offset;
                //double width = _width + offset;
                //builder
                //    .MoveTo(new(-inner, _height)).LineTo(new(inner, _height))
                //    .CurveTo(new(inner + _kx, _height), new(width, _ky), new(width, 0))
                //    .SmoothTo(new(inner + _kx, -_height), new(inner, -_height))
                //    .LineTo(new(-inner, -_height))
                //    .CurveTo(new(-inner - _kx, -_height), new(-width, -_ky), new(-width, 0))
                //    .SmoothTo(new(-inner - _kx, _height), new(-inner, _height))
                //    .SmoothTo(new(-inner + _rx, _ky), new(-inner + _rx, 0))
                //    .SmoothTo(new(-inner + _kx, -_height), new(-inner, -_height));
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
                Pins.Add(new FixedOrientedPin("left", "The left signal.", this, new(), new(-1, 0)), "a", "l");
                Pins.Add(new FixedOrientedPin("leftground", "The left ground.", this, new(), new(0, 1)), "ga", "gl");
                Pins.Add(new FixedOrientedPin("rightground", "The right ground.", this, new(), new(0, 1)), "gb", "gr");
                Pins.Add(new FixedOrientedPin("right", "The right signal.", this, new(), new(1, 0)), "b", "r");
            }

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Sizes:

                        // Calculate sizes
                        var style = context.Style.ModifyDashedDotted(this);
                        var labelBounds = LabelAnchorPoints<IDrawable>.CalculateBounds(context.TextFormatter, Labels, 2, _anchors, style);
                        _width = Width.IsZero() ? Math.Max(labelBounds.Height + Margin.Top + Margin.Bottom, MinWidth) : Width;
                        _rx = _width * 0.25;
                        _length = Length.IsZero() ? Math.Max(labelBounds.Width + Margin.Left + Margin.Right + 2 * _rx, MinLength) : Length;

                        // Update the pins
                        SetPinOffset(0, new(_rx - _length * 0.5, 0.0));
                        SetPinOffset(1, new(_rx - _length * 0.5, _width * 0.5));
                        SetPinOffset(2, new(_length * 0.5, _width * 0.5));
                        SetPinOffset(3, new(_length * 0.5, 0.0));

                        // Calculate the anchor positions
                        _anchors[0] = new(new(0, -_width * 0.5 - 1), new(0, -1));
                        _anchors[1] = new(new(0, _width * 0.5 + 1), new(0, 1));
                        _anchors[2] = new(new(0, 0), new(0, 0), new(new(1, 0), TextOrientationTypes.Transformed));
                        break;
                }

                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);

                // Transmission line
                builder.Path(DrawShape, style);

                // Wire
                builder.ExtendPins(Pins, style, 2, "a", "b");

                _anchors.Draw(builder, this, style);
            }
        }
    }
}