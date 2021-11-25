using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes a component with a graphical representation, and pins that can be interconnected.
    /// </summary>
    /// <seealso cref="IItem" />
    public interface IDrawable : ICircuitPresence
    {
        /// <summary>
        /// Gets the pins.
        /// </summary>
        public IPinCollection Pins { get; }

        /// <summary>
        /// Gets the set of variants for this drawable.
        /// </summary>
        public ISet<string> Variants { get; }

        /// <summary>
        /// Gives the drawable an order in which it can be drawn. This can be used
        /// to force some components to be drawn over others.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Renders the component in the specified drawing.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        /// <param name="drawing">The drawing.</param>
        public void Render(SvgDrawing drawing);
    }
}
