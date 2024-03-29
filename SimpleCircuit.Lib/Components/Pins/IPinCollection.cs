﻿using System.Collections.Generic;

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
        /// <returns>The pin; or <c>null</c> if the pin doesn't exist.</returns>
        public IPin this[string name] { get; }

        /// <summary>
        /// Gets the pin by its index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The pin; or <c>null</c> if the pin doesn't exist.</returns>
        public IPin this[int index] { get; }

        /// <summary>
        /// Gets the names of the pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <returns>The names.</returns>
        public IEnumerable<string> NamesOf(IPin pin);

        /// <summary>
        /// Tries to get a pin.
        /// </summary>
        /// <param name="name">The name of the pin.</param>
        /// <param name="pin">The pin.</param>
        /// <returns>Returns <c>true</c> if the pin was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string name, out IPin pin);

        /// <summary>
        /// Clears the pin collection.
        /// </summary>
        public void Clear();
    }
}
