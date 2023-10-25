using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Metadata describing a drawable.
    /// </summary>
    public class DrawableMetadata
    {
        /// <summary>
        /// Gets a key describing a drawable.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the description of the drawable.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the category of the drawable.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Gets keywords associated with the drawable.
        /// </summary>
        public HashSet<string> Keywords { get; } = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Creates a new metadata.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="category">The category.</param>
        public DrawableMetadata(string key, string description, string category = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Category = category ?? "General";
        }
    }
}
