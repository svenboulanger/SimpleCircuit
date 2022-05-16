using System;

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
        public string[] Keys { get; }

        /// <summary>
        /// Gets the description of the drawable.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the category of the drawable.
        /// </summary>
        public string[] Categories { get; }

        /// <summary>
        /// Creates a new metadata.
        /// </summary>
        /// <param name="keys">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="categories">The category.</param>
        public DrawableMetadata(string[] keys, string description, string[] categories = null)
        {
            Keys = keys ?? throw new ArgumentNullException(nameof(keys));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Categories = categories ?? new[] { "General" };
        }
    }
}
