using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes a component with a graphical representation, and pins that can be interconnected.
    /// </summary>
    public interface IDrawable : ICircuitSolverPresence
    {
        /// <summary>
        /// Gets the variants.
        /// </summary>
        public VariantSet Variants { get; }

        /// <summary>
        /// Gets the pins.
        /// </summary>
        public IPinCollection Pins { get; }

        /// <summary>
        /// Renders the component in the specified drawing.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        /// <param name="drawing">The drawing.</param>
        public void Render(SvgDrawing drawing);
    }
}
