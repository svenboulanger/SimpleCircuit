using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// ADC.
    /// </summary>
    [SimpleKey("ADC", "Analog-to-digital converter", Category = "Digital")]
    public class AnalogToDigital : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogToDigital"/>
        /// </summary>
        /// <param name="name"></param>
        public AnalogToDigital(string name)
            : base(name)
        {
            Pins.Add(new[] { "in" }, "Input", new Vector2(-9, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "out" }, "Output", new Vector2(9, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new[]
            {
                new Vector2(-9, 6), new Vector2(3, 6),
                new Vector2(9, 0), new Vector2(3, -6),
                new Vector2(-9, -6)
            });

            if (!string.IsNullOrWhiteSpace(Label))
            {
                drawing.Text(Label, new Vector2(-10, 0), new Vector2(1, 0));
            }
        }

        /// <summary>
        /// Converts to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => $"ADC {Name}";
    }
}
