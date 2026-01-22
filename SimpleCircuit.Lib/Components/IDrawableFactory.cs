using SimpleCircuit.Diagnostics;
using SimpleCircuit.Evaluator;
using System.Collections.Generic;

namespace SimpleCircuit.Components;

/// <summary>
/// An interface that describes a factory for a certain type of drawable.
/// </summary>
public interface IDrawableFactory
{
    /// <summary>
    /// Gets the keys that the factory supports.
    /// </summary>
    public IEnumerable<string> Keys { get; }

    /// <summary>
    /// Gets the metadata for the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The metadata, or <c>null</c> if the drawable does not support the key.</returns>
    public DrawableMetadata GetMetadata(string key);

    /// <summary>
    /// Creates a drawable with the specified name.
    /// </summary>
    /// <param name="key">The key for which the factory should create an instance.</param>
    /// <param name="name">The name of the instance.</param>
    /// <param name="options">The options currently in use.</param>
    /// <param name="diagnostics">The diagnostics handler.</param>
    /// <returns>The drawable, or <c>null</c> if it couldn't be created.</returns>
    public IDrawable Create(string key, string name, Options options, Scope scope, IDiagnosticHandler diagnostics);
}
