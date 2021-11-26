using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// ADC.
    /// </summary>
    [SimpleKey("ADC", "An Analog-to-digital converter.", Category = "Digital")]
    public class AnalogToDigital : ScaledOrientedDrawable, ILabeled
    {
        /// <summary>
        /// Gets or sets the width of the ADC.
        /// </summary>
        [Description("The width of the ADC.")]
        public double Width { get; set; } = 18;

        /// <summary>
        /// Gets or sets the height of the ADC.
        /// </summary>
        [Description("The height of the ADC.")]
        public double Height { get; set; } = 12;

        /// <inheritdoc/>
        [Description("The label inside the ADC.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogToDigital"/>
        /// </summary>
        /// <param name="name">The name of the ADC.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public AnalogToDigital(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, new(-9, 0), new(-1, 0)), "input", "in", "pi", "inp");
            Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, new(-9, 0), new(-1, 0)), "inn", "ni");
            Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(9, 0), new(1, 0)), "outn", "no");
            Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, new(9, 0), new(1, 0)), "output", "out", "po", "outp");

            PinUpdate = Variant.All(
                Variant.Map("diffin", RedefineInputPins),
                Variant.Map("diffout", RedefineOutputPins));
            DrawingVariants = Variant.Do<SvgDrawing>(DrawADC);
        }

        private void DrawADC(SvgDrawing drawing)
        {
            drawing.Polygon(new[]
            {
                new Vector2(-Width / 2, Height / 2), new Vector2(Width / 2 - Height / 2, Height / 2),
                new Vector2(Width / 2, 0), new Vector2(Width / 2 - Height / 2, -Height / 2),
                new Vector2(-Width / 2, -Height / 2)
            });

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(-Height / 4, 0), new Vector2(0, 0));
        }

        private void SetPinOffset(int index, Vector2 offset)
            => ((FixedOrientedPin)Pins[index]).Offset = offset;
        private void RedefineInputPins(bool differential)
        {
            SetPinOffset(0, differential ? new(-Width / 2, -Height / 4) : new(-Width / 2, 0));
            SetPinOffset(1, differential ? new(-Width / 2, Height / 4) : new(-Width / 2, 0));
        }
        private void RedefineOutputPins(bool differential)
        {
            SetPinOffset(2, differential ? new(Width / 2 - Height / 4, Height / 4) : new(Width / 2, 0));
            SetPinOffset(3, differential ? new(Width / 2 - Height / 4, -Height / 4) : new(Width / 2, 0));
        }

        /// <summary>
        /// Converts to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => $"ADC {Name}";
    }
}
