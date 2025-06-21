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
    /// An impedance/admittance.
    /// </summary>
    [Drawable("Z", "An impedance.", "Analog", "programmable")]
    [Drawable("Y", "An admittance.", "Analog", "programmable")]
    public class Impedance : DrawableFactory
    {
        private const string _programmable = "programmable";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(key == "Y" ? "admittance" : "impedance", name);

        private class Instance : ScaledOrientedDrawable
        {
            private double _width, _length;
            private readonly CustomLabelAnchorPoints _anchors = new(3);

            /// <inheritdoc />
            public override string Type { get; }

            [Description("The margin of the label inside the ADC when sizing.")]
            public Margins Margin { get; set; } = new(2, 2, 2, 2);

            [Description("The length of the impedance symbol. If 0, the length is computed based on the center label and the minimum length.")]
            [Alias("l")]
            public double Length { get; set; } = 0;

            [Description("The width of the impedance symbol. If 0, the width is computed based on the center label and the minimum width.")]
            [Alias("w")]
            public double Width { get; set; } = 0;

            [Description("The minimum length of the symbol. The default is 6.")]
            [Alias("ml")]
            public double MinLength { get; set; } = 10.0;

            [Description("The minimum width of the symbol. The default is 4.")]
            [Alias("mw")]
            public double MinWidth { get; set; } = 4.0;

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

            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Sizes:
                        // Calculate the label bounds
                        var style = context.Style.ModifyDashedDotted(this);
                        var labelBounds = LabelAnchorPoints<IDrawable>.CalculateBounds(context.TextFormatter, this, 1, _anchors, style);

                        // Determine the height
                        _width = Width.IsZero() ? Math.Max(labelBounds.Height + Margin.Top + Margin.Bottom, MinWidth) : Width;

                        // Determine the length
                        _length = Length.IsZero() ? Math.Max(labelBounds.Width + Margin.Left + Margin.Right, MinLength) : Length;

                        // Update the pins
                        SetPinOffset(0, new(-_length * 0.5, 0.0));
                        SetPinOffset(1, new(_length * 0.5, 0.0));

                        // Set the anchors
                        _anchors[1] = new(default, default, Vector2.UX, TextOrientationType.Transformed);
                        if (Variants.Contains(_programmable))
                        {
                            _anchors[0] = new(new(0, -_width * 0.5 - 5), new(0, -1));
                            _anchors[2] = new(new(0, _width * 0.5 + 2), new(0, 1));
                        }
                        else
                        {
                            _anchors[0] = new(new(0, -_width * 0.5 - 1), new(0, -1));
                            _anchors[2] = new(new(0, _width * 0.5 + 1), new(0, 1));
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                builder.ExtendPins(Pins, style);

                // The rectangle
                double w = _width * 0.5;
                builder.Rectangle(-_length * 0.5, -w, _length, _width, style);

                if (Variants.Contains(_programmable))
                    builder.Arrow(new(-5, w + 1), new(6, -w - 4), style);
                _anchors.Draw(builder, this, style);
            }
        }
    }
}
