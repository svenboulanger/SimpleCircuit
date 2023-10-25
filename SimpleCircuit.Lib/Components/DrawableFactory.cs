using SimpleCircuit.Diagnostics;
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
        private readonly Dictionary<string, DrawableMetadata> _metadata = new();

        /// <inheritdoc />
        public IEnumerable<string> Keys => _metadata.Keys;

        /// <summary>
        /// Creates a new drawable factory.
        /// </summary>
        public DrawableFactory()
        {
            // Populate the metadata
            foreach (var attribute in GetType().GetCustomAttributes(false).OfType<DrawableAttribute>())
            {
                _metadata[attribute.Key] = new DrawableMetadata(attribute.Key, attribute.Description, attribute.Category);
            }
        }

        /// <inheritdoc />
        public DrawableMetadata GetMetadata(string key)
        {
            if (_metadata.TryGetValue(key, out var metadata))
                return metadata;
            return null;
        }

        /// <summary>
        /// Creates a new instance of the drawable.
        /// </summary>
        /// <param name="key">The key of the drawable being created.</param>
        /// <param name="name">The name of the drawable being created.</param>
        /// <returns>The drawable.</returns>
        protected abstract IDrawable Factory(string key, string name);

        /// <inheritdoc />
        public virtual IDrawable Create(string key, string name, Options options, IDiagnosticHandler diagnostics)
        {
            var result = Factory(key, name);
            if (options != null)
            {
                if (result is IScaledDrawable scaled)
                    scaled.Scale = options.Scale;
                if (result is IStandardizedDrawable standardized)
                {
                    switch (options.Standard)
                    {
                        case Standards.AREI:
                            if ((standardized.Supported & Standards.AREI) == Standards.AREI)
                                standardized.Variants.Add(Options.Arei);
                            else
                                goto default;
                            break;

                        case Standards.European:
                            if ((standardized.Supported & Standards.European) == Standards.European)
                                standardized.Variants.Add(Options.European);
                            else
                                goto default;
                            break;

                        case Standards.American:
                            if ((standardized.Supported & Standards.American) == Standards.American)
                                standardized.Variants.Add(Options.American);
                            else
                                goto default;
                            break;

                        default:
                            break;
                    }
                }
                options.Apply(key, result, diagnostics);
            }
            return result;
        }
    }
}
