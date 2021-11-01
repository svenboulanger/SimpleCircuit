using SimpleCircuit.Components.Pins;

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
        /// Renders the component in the specified drawing.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        /// <param name="drawing">The drawing.</param>
        public void Render(SvgDrawing drawing);
    }
}
