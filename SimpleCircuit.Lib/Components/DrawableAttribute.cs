using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// An attribute used by the <see cref="DrawableFactory"/> class to automatically
    /// populate the different metadata.
    /// </summary>
    /// <remarks>
    /// Creates a new attribute.
    /// </remarks>
    /// <param name="key">The key.</param>
    /// <param name="description">The description.</param>
    /// <param name="category">The category.</param>
    /// <param name="keywords">The keywords.</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DrawableAttribute(string key, string description, string category, string keywords = null) : Attribute
    {
        /// <summary>
        /// Gets the key of the drawable.
        /// </summary>
        public string Key { get; } = key;

        /// <summary>
        /// Gets the description of the attribute.
        /// </summary>
        public string Description { get; } = description;

        /// <summary>
        /// Gets the category of the attribute.
        /// </summary>
        public string Category { get; } = category;

        /// <summary>
        /// Gets the keywords of the attribute.
        /// </summary>
        public string Keywords { get; } = keywords;
    }
}
