using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// An attribute used by the <see cref="DrawableFactory"/> class to automatically
    /// populate the different metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DrawableAttribute : Attribute
    {
        /// <summary>
        /// Gets the key of the drawable.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the description of the attribute.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the category of the attribute.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Creates a new attribute.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="category">The category.</param>
        public DrawableAttribute(string key, string description, string category)
        {
            Key = key;
            Description = description;
            Category = category;
        }
    }
}
