using System;

namespace SimpleCircuit;

/// <summary>
/// An attribute that indicates an alias for a property.
/// </summary>
/// <remarks>
/// Creates a new alias attribute.
/// </remarks>
/// <param name="alias">The alias.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class AliasAttribute(string alias) : Attribute
{
    /// <summary>
    /// Gets the alias.
    /// </summary>
    public string Alias { get; } = alias ?? string.Empty;
}
