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
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets whether the squiggly line needs to be drawn.
        /// </summary>
        public bool AC { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public VoltageSource(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-8, 0), new(-1, 0)), "n", "neg", "b");
            Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(8, 0), new(1, 0)), "p", "pos", "a");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Circle(new Vector2(0, 0), 6);
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 0), new Vector2(8, 0),
            });

            if (AC)
            {
                double handle = 1.7 / Math.Sqrt(2); // Slighly exaggerated
                drawing.OpenBezier(new Vector2[]
                {
                        new(0, -3),
                        new(handle, -3 + handle), new(handle, -handle), new(),
                        new(-handle, handle), new(-handle, 3 - handle), new(0, 3)
                });
            }
            else
            {
                drawing.Segments(new Vector2[] {
                        new(-3, -1), new(-3, 1),
                        new(3, -1), new(3, 1),
                        new(2, 0), new(4, 0),
                    });
            }

            // Depending on the orientation, let's anchor the text differently
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -8), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Voltage source {Name}";
    }
}