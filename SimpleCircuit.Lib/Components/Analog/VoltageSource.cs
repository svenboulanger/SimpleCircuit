using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    [SimpleKey("V", "A voltage source.", Category = "Analog")]
    public class VoltageSource : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the source.")]
        public string Label { get; set; }

        [Description("Makes the voltage-source an AC source.")]
        public bool AC { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public VoltageSource(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-8, 0), new(-1, 0)), "n", "neg", "b");
            Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(8, 0), new(1, 0)), "p", "pos", "a");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new Vector2[]
            {
                new(-8, 0), new(-6, 0),
                new(6, 0), new(8, 0),
            }, new("wire"));

            // Circle
            drawing.Circle(new(0, 0), 6);
            if (AC)
            {
                double handle = 1.7 / Math.Sqrt(2); // Slighly exaggerated curves
                drawing.OpenBezier(new Vector2[]
                {
                    new(0, -3),
                    new(handle, -3 + handle), new(handle, -handle), new(),
                    new(-handle, handle), new(-handle, 3 - handle), new(0, 3)
                });
            }
            else
            {
                // Plus and minus
                drawing.Line(new(-3, -1), new(-3, 1), new("minus"));
                drawing.Segments(new Vector2[]
                {
                    new(3, -1), new(3, 1),
                    new(2, 0), new(4, 0)
                }, new("plus"));
            }

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -8), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Voltage source {Name}";
    }
}