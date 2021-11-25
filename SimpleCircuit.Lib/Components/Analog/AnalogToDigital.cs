using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;

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
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new[]
            {
                new Vector2(-Width / 2, Height / 2), new Vector2(Width / 2 - Height / 2, Height / 2),
                new Vector2(Width / 2, 0), new Vector2(Width / 2 - Height / 2, -Height / 2),
                new Vector2(-Width / 2, -Height / 2)
            });

            if (!string.IsNullOrWhiteSpace(Label))
            {
                drawing.Text(Label, new Vector2(-Height / 4, 0), new Vector2(0, 0));
            }
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            var pin1 = (FixedOrientedPin)Pins[0];
            var pin2 = (FixedOrientedPin)Pins[1];
            if (Variants.Contains("diffin"))
            {
                pin1.Offset = new(-Width / 2, -Height / 4);
                pin2.Offset = new(-Width / 2, Height / 4);
            }
            else
            {
                pin1.Offset = new(-Width / 2, 0);
                pin2.Offset = new(-Width / 2, 0);
            }

            pin1 = (FixedOrientedPin)Pins[2];
            pin2 = (FixedOrientedPin)Pins[3];
            if (Variants.Contains("diffout"))
            {
                pin1.Offset = new(Width / 2 - Height / 4, Height / 4);
                pin2.Offset = new(Width / 2 - Height / 4, -Height / 4);
            }
            else
            {
                pin1.Offset = new(Width / 2, 0);
                pin2.Offset = new(Width / 2, 0);
            }

            base.DiscoverNodeRelationships(context, diagnostics);
        }

        /// <summary>
        /// Converts to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => $"ADC {Name}";
    }
}
