using System;

namespace SimpleCircuit.Components;

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
/// <param name="labelCount">The most number of labels that can be placed at different anchor points. Used for generating demo's.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DrawableAttribute(string key, string description, string category, string keywords = null, int labelCount = 1) : Attribute
{
    /// <summary>
    /// Gets the key of the drawable.
    /// </summary>
    public string Key => key;

    /// <summary>
    /// Gets the description of the attribute.
    /// </summary>
    public string Description => description;

    /// <summary>
    /// Gets the category of the attribute.
    /// </summary>
    public string Category => category;

    /// <summary>
    /// Gets the keywords of the attribute.
    /// </summary>
    public string Keywords => keywords;

    public int LabelCount => labelCount;
}
