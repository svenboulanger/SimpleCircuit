using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
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
        /// <returns>The drawable, or <c>null</c> if it couldn't be created.</returns>
        public IDrawable Create(string key, string name, Options options, IDiagnosticHandler diagnostics);
    }
}
