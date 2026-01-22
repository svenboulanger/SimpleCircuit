using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Drawing.Builders;

namespace SimpleCircuit.Components.Analog;

/// <summary>
/// An analog-to-digital converter.
/// </summary>
[Drawable("ADC", "An analog-to-digital converter.", "Digital", labelCount: 3)]
public class AnalogToDigital : DrawableFactory
{
    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(3);

        private const string _differentialInput = "diffin";
        private const string _swapInput = "swapin";
        private const string _differentialOutput = "diffout";
        private const string _swapOutput = "swapout";
        private double _width = 0.0, _height = 0.0;

        [Description("The margin of the label inside the ADC when sizing based on content.")]
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

        [Description("The label margin. The default is 1.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

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
                    break;

                case PreparationMode.Sizes:
                    // Calculate the label bounds
                    var style = context.Style.ModifyDashedDotted(this);
                    var labelBounds = LabelAnchorPoints<IDrawable>.CalculateBounds(context.TextFormatter, this, 0, _anchors, style);

                    // Determine the height
                    _height = Height.IsZero() ? Math.Max(labelBounds.Height + Margin.Vertical, MinHeight) : Height;

                    // Determine the width
                    if (Width.IsZero())
                    {
                        _width = Math.Max(labelBounds.Width + Margin.Horizontal, MinWidth);
                        if (Variants.Contains(_differentialInput))
                            _width += 4.0;
                        _width += 0.5 * _height;
                    }
                    else
                        _width = Width;

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

                    double m = style.LineThickness * 0.5 + LabelMargin;
                    _anchors[0] = new LabelAnchorPoint(new(x, 0), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.MiddleBegin);
                    _anchors[1] = new LabelAnchorPoint(new(0, -_height * 0.5 - m), new(0, -1));
                    _anchors[2] = new LabelAnchorPoint(new(0, _height * 0.5 + m), new(0, 1));
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

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
            ], style);

            // Inputs
            if (Variants.Contains(_differentialInput))
            {
                builder.ExtendPins(Pins, style, 2, "inp", "inn");
                if (Variants.Contains(_swapInput))
                    builder.Signs(new(-w + 3, 0.5 * h), new(-w + 3, -0.5 * h), style);
                else
                    builder.Signs(new(-w + 3, -0.5 * h), new(-w + 3, 0.5 * h), style);
            }
            else
                builder.ExtendPin(Pins["in"], style);

            // Outputs
            if (Variants.Contains(_differentialOutput))
            {
                builder.ExtendPins(Pins, style, 4, "outp", "outn");
                x = w - 0.25 * h;
                double y = 0.5 * h + 2;
                if (Variants.Contains(_swapOutput))
                    builder.Signs(new(x, y), new(x, -y), style);
                else
                    builder.Signs(new(x, -y), new(x, y), style);
            }
            else
                builder.ExtendPin(Pins["out"], style);

            // Labels
            _anchors.Draw(builder, this, style);
        }
    }
}