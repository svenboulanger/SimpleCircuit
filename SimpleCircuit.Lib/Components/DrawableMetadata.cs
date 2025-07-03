using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Metadata describing a drawable.
    /// </summary>
    /// <remarks>
    /// Creates a new metadata.
    /// </remarks>
    /// <param name="key">The key.</param>
    /// <param name="description">The description.</param>
    /// <param name="category">The category.</param>
    public class DrawableMetadata(string key, string description, int labelCount, string category = null)
    {
        /// <summary>
        /// Gets a key describing a drawable.
        /// </summary>
        public string Key { get; } = key ?? throw new ArgumentNullException(nameof(key));

        /// <summary>
        /// Gets the description of the drawable.
        /// </summary>
        public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

        /// <summary>
        /// Gets the category of the drawable.
        /// </summary>
        public string Category { get; } = category ?? "General";

        /// <summary>
        /// Gets keywords associated with the drawable.
        /// </summary>
        public HashSet<string> Keywords { get; } = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets the number of labels.
        /// </summary>
        public int LabelCount => labelCount;
    }
}
