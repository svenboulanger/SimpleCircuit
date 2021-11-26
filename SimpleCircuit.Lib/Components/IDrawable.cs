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
        /// Gives the drawable an order in which it can be drawn. This can be used
        /// to force some components to be drawn over others.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Adds a variant for the drawable.
        /// </summary>
        /// <param name="variant">The variant.</param>
        public void AddVariant(string variant);

        /// <summary>
        /// Removes a variant from the drawable.
        /// </summary>
        /// <param name="variant">The variant.</param>
        public void RemoveVariant(string variant);

        /// <summary>
        /// Renders the component in the specified drawing.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        /// <param name="drawing">The drawing.</param>
        public void Render(SvgDrawing drawing);
    }
}
