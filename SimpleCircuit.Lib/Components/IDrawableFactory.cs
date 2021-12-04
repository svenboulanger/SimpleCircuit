using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// A standard implementation for an <see cref="IDrawableFactory"/> that will look
    /// for <see cref="DrawableAttribute" /> attributes to populate its metadata.
    /// </summary>
    public abstract class DrawableFactory : IDrawableFactory
    {
        private readonly List<DrawableMetadata> _metadata = new();

        /// <inheritdoc />
        public IEnumerable<DrawableMetadata> Metadata => _metadata;

        /// <summary>
        /// Creates a new drawable factory.
        /// </summary>
        public DrawableFactory()
        {
            // Populate the metadata
            foreach (var attribute in GetType().GetCustomAttributes(false).OfType<DrawableAttribute>())
                _metadata.Add(attribute.Metadata);
        }

        /// <inheritdoc />
        public abstract IDrawable Create(string key, string name, Options options);
    }

    /// <summary>
    /// An interface that describes a factory for a certain type of drawable.
    /// </summary>
    public interface IDrawableFactory
    {
        /// <summary>
        /// Gets the key for a component of this type.
        /// </summary>
        public IEnumerable<DrawableMetadata> Metadata { get; }

        /// <summary>
        /// Creates a drawable with the specified name.
        /// </summary>
        /// <param name="name">The name of the factory, which starts with a key given by the <see cref="Keys"/> property.</param>
        /// <returns>The drawable.</returns>
        public IDrawable Create(string key, string name, Options options);
    }
}
