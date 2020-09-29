using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes an instance that be translated.
    /// </summary>
    public interface ITranslating
    {
        /// <summary>
        /// Gets the x-coordinate of the component.
        /// </summary>
        /// <value>
        /// The x-coordinate.
        /// </value>
        Function X { get; }

        /// <summary>
        /// Gets the y-coordinate of the component.
        /// </summary>
        /// <value>
        /// The y-coordinate.
        /// </value>
        Function Y { get; }
    }
}
