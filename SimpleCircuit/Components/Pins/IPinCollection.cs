using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes a collection of pins.
    /// </summary>
    public interface IPinCollection : IEnumerable<IPin>
    {
        /// <summary>
        /// Gets the number of pins.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a pin by name.
        /// </summary>
        /// <param name="name">The name of the pin.</param>
        /// <returns>The pin.</returns>
        IPin this[string name] { get; }

        /// <summary>
        /// Gets a pin by index.
        /// </summary>
        /// <param name="index">The index of the pin.</param>
        /// <returns>The pin.</returns>
        IPin this[int index] { get; }

        /// <summary>
        /// Enumerates the names of the specified name.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <returns>The names of the pin.</returns>
        IEnumerable<string> NamesOf(IPin pin);
    }
}
