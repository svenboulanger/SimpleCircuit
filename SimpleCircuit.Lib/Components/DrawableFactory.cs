﻿using SimpleCircuit.Diagnostics;
using SimpleCircuit.Evaluator;
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
        private readonly Dictionary<string, DrawableMetadata> _metadata = [];

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
                var metadata = _metadata[attribute.Key] = new DrawableMetadata(attribute.Key, attribute.Description, attribute.Category);
                if (!string.IsNullOrWhiteSpace(attribute.Keywords))
                {
                    foreach (string keyword in attribute.Keywords.Split(new[] { ' ', ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries))
                        metadata.Keywords.Add(keyword);
                }
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
        public virtual IDrawable Create(string key, string name, Options options, Scope scope, IDiagnosticHandler diagnostics)
        {
            var result = Factory(key, name);
            scope?.ApplyDefaults(name, result, diagnostics);
            return result;
        }
    }
}
