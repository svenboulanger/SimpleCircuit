using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components.General
{
    /// <summary>
    /// A direction that is like a regular point, but can be oriented.
    /// This is useful for example when combined with subcircuits to give an orientation.
    /// </summary>
    [SimpleKey("DIR", "Directional point, useful for subcircuit definitions or indicating busses (using crossings).", Category = "General")]
    public class Direction : OrientedDrawable
    {
        /// <summary>
        /// Initializers a new instance of the <see cref="Direction"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Direction(string name, Options options)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("input", "The input.", this, new(), new(-1, 0)), "i", "a", "in", "input");
            Pins.Add(new FixedOrientedPin("output", "The output.", this, new(), new(1, 0)), "o", "b", "out", "output");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
        }

        /// <summary>
        /// Converts to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"Direction {Name}";
    }
}
