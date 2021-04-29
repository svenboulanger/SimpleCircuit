using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A component with a width and height.
    /// </summary>
    public interface ISizeable
    {
        /// <summary>
        /// Gets the width of the component.
        /// </summary>
        Function Width { get; }

        /// <summary>
        /// Gets the height of the component.
        /// </summary>
        Function Height { get; }
    }
}
