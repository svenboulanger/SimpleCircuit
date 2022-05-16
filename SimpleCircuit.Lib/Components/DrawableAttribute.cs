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
        public string[] Keys { get; }

        /// <summary>
        /// Gets the description of the attribute.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the category of the attribute.
        /// </summary>
        public string[] Categories { get; }

        /// <summary>
        /// Gets the metadata for the attribute.
        /// </summary>
        public DrawableMetadata Metadata => new(Keys, Description, Categories);

        /// <summary>
        /// Creates a new attribute.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="category">The category.</param>
        public DrawableAttribute(string key, string description, string category)
        {
            Keys = new[] { key };
            Description = description;
            Categories = new[] { category };
        }

        /// <summary>
        /// Creates a new attribute.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="description">The description.</param>
        /// <param name="categories">The categories.</param>
        public DrawableAttribute(string[] keys, string description, string[] categories)
        {
            Keys = keys;
            Description = description;
            Categories = categories;
        }
    }
}
