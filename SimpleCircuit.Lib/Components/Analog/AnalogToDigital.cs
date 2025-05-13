using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An analog-to-digital converter.
    /// </summary>
    [Drawable("ADC", "An analog-to-digital converter.", "Digital")]
    public class AnalogToDigital : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            private const string _differentialInput = "diffin";
            private const string _swapInput = "swapin";
            private const string _differentialOutput = "diffout";
            private const string _swapOutput = "swapout";
            private double _width = 0.0, _height = 0.0;

            [Description("The margin of the label inside the ADC when sizing.")]
            public Margins Margin { get; set; } = new(2, 2, 2, 2);

            [Description("The width of the ADC. If 0, the width is computed based on the label and minimum width. Default is 0.")]
            [Alias("w")]
            public double Width { get; set; } = 0;

            [Description("The height of the ADC. If 0, the height is computed based on the label and minimum height. Default is 0.")]
            [Alias("h")]
            public double Height { get; set; } = 0;

            [Description("The minimum width. The default is 18.")]
            [Alias("mw")]
            public double MinWidth { get; set; } = 18;

            [Description("The minimum height. The defalut is 12.")]
            [Alias("mh")]
            public double MinHeight { get; set; } = 12;

            /// <inheritdoc />
            public override string Type => "adc";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        Pins.Clear();

                        double x = -Width / 2;
                        double y = Height / 4;

                        // Inputs
                        if (Variants.Contains(_differentialInput))
                        {
                            Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input", this, default, new(-1, 0)), "i", "in", "inp", "pi", "p");
                            Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, default, new(-1, 0)), "inn", "ni", "n");
                        }
                        else
                            Pins.Add(new FixedOrientedPin("input", "The input.", this, default, new(-1, 0)), "i", "in", "inp", "pi", "p");

                        if (Variants.Contains(_differentialOutput))
                        {
                            Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, default, new(1, 0)), "o", "out", "outp", "po");
                            Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, default, new(1, 0)), "outn", "no");
                        }
                        else
                            Pins.Add(new FixedOrientedPin("output", "The output.", this, default, new(1, 0)), "o", "out", "outp", "po");

                        // Allow dashed/dotted lines
                        Appearance.LineStyle = Variants.Select(Dashed, Dotted) switch
                        {
                            0 => LineStyles.Dashed,
                            1 => LineStyles.Dotted,
                            _ => LineStyles.None
                        };
                        break;

                    case PreparationMode.Sizes:
                        // Calculate the size of the total entity
                        double labelX = 0.0, labelY = Math.Max(Height, MinHeight);
                        if (Width.IsZero() || Height.IsZero())
                        {
                            // Go through the labels and find the one in the middle that will determine the height/width
                            for (int i = 0; i < Labels.Count; i++)
                            {
                                var bounds = Labels[i].Formatted.Bounds.Bounds;
                                if (Width.IsZero())
                                    _width = Math.Max(_width, bounds.Width);
                                if (Height.IsZero())
                                    _height = Math.Max(_height, bounds.Height);
                                labelX = Math.Min(labelX, bounds.Left);
                                labelY = Math.Min(labelY, bounds.Height * 0.5 - bounds.Bottom);
                            }

                            // The true width adds half of the height
                            if (Height.IsZero())
                                _height = Math.Max(_height + Margin.Top + Margin.Bottom, MinHeight);
                            else
                                _height = Height;
                            if (Width.IsZero())
                            {
                                if (Variants.Contains(_differentialInput))
                                    _width += 4.0; // Accommodate for the signs
                                _width += 0.5 * _height; // Accommodate for the triangle output shape
                                _width = Math.Max(_width + Margin.Left + Margin.Right, MinWidth);
                            }
                            else
                            {
                                _width = Width;
                                if (_width < _height * 0.5)
                                    _width = _height * 0.5;
                            }
                        }
                        else
                        {
                            _width = Width;
                            _height = Height;

                            for (int i = 0; i < Labels.Count; i++)
                            {
                                var bounds = Labels[i].Formatted.Bounds.Bounds;
                                labelX = Math.Min(labelX, bounds.Left);
                                labelY = Math.Min(labelY, bounds.Height * 0.5 - bounds.Bottom);
                            }
                        }

                        // Update the pins
                        int index = 0;
                        if (Variants.Contains(_differentialInput))
                        {
                            if (Variants.Contains(_swapInput))
                            {
                                SetPinOffset(0, new(-_width * 0.5, -_height * 0.25));
                                SetPinOffset(1, new(-_width * 0.5, _height * 0.25));
                            }
                            else
                            {
                                SetPinOffset(0, new(-_width * 0.5, _height * 0.25));
                                SetPinOffset(1, new(-_width * 0.5, -_height * 0.25));
                            }
                            index += 1;
                        }
                        else
                            SetPinOffset(0, new(-_width * 0.5, 0));

                        index++;
                        if (Variants.Contains(_differentialOutput))
                        {
                            x = _width * 0.5 - _height * 0.25;
                            if (Variants.Contains(_swapOutput))
                            {
                                SetPinOffset(index, new(x, -_height * 0.25));
                                SetPinOffset(index + 1, new(x, _height * 0.25));
                            }
                            else
                            {
                                SetPinOffset(index, new(x, _height * 0.25));
                                SetPinOffset(index + 1, new(x, -_height * 0.25));
                            }
                        }
                        else
                            SetPinOffset(index, new(_width * 0.5, 0));

                        // Set the anchors
                        x = -_width * 0.5 + Margin.Left;
                        if (Variants.Contains(_differentialInput))
                            x += 4;
                        _anchors[0] = new LabelAnchorPoint(new(x - labelX, labelY), new(1, 0), Appearance, true);
                        _anchors[1] = new LabelAnchorPoint(new(-_width * 0.5, -_height * 0.5 - Margin.Top), new(1, -1), Appearance);
                        _anchors[2] = new LabelAnchorPoint(new(-_width * 0.5, _height * 0.5 + Margin.Bottom), new(1, 1), Appearance);
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                // ADC base shape
                double w = 0.5 * _width;
                double h = 0.5 * _height;
                double x = 0.5 * (_width - _height);
                builder.Polygon(
                [
                    new(-w , h),
                    new(x, h),
                    new(w, 0),
                    new(x, -h),
                    new(-w, -h)
                ], Appearance);

                // Inputs
                if (Variants.Contains(_differentialInput))
                {
                    builder.ExtendPins(Pins, Appearance, 2, "inp", "inn");
                    if (Variants.Contains(_swapInput))
                        builder.Signs(new(-w + 3, 0.5 * h), new(-w + 3, -0.5 * h), Appearance);
                    else
                        builder.Signs(new(-w + 3, -0.5 * h), new(-w + 3, 0.5 * h), Appearance);
                }
                else
                    builder.ExtendPin(Pins["in"], Appearance);

                // Outputs
                if (Variants.Contains(_differentialOutput))
                {
                    builder.ExtendPins(Pins, Appearance, 4, "outp", "outn");
                    x = w - 0.25 * h;
                    double y = 0.25 * h + 3;
                    if (Variants.Contains(_swapOutput))
                        builder.Signs(new(x, y), new(x, -y), Appearance);
                    else
                        builder.Signs(new(x, -y), new(x, y), Appearance);
                }
                else
                    builder.ExtendPin(Pins["out"], Appearance);

                // Labels
                _anchors.Draw(builder, this);
            }
        }
    }
}