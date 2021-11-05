using System.Collections.Generic;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A pin collection.
    /// </summary>
    public interface IPinCollection : IEnumerable<IPin>
    {
        /// <summary>
        /// Gets the number of pins in the collection.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets the pin by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The pin.</returns>
        public IPin this[string name] { get; }

        /// <summary>
        /// Gets the pin by its index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The pin index.</returns>
        public IPin this[int index] { get; }

        /// <summary>
        /// Gets the names of the pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <returns>The names.</returns>
        public IEnumerable<string> NamesOf(IPin pin);
    }
}
