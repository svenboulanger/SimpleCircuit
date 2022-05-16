using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components
{
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
}
