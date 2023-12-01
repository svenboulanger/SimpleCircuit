using System;

namespace SimpleCircuit
{
    /// <summary>
    /// An attribute that indicates the description of a property.
    /// </summary>
    /// <remarks>
    /// Creates a new description attribute.
    /// </remarks>
    /// <param name="description">The description.</param>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DescriptionAttribute(string description) : Attribute
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; } = description ?? string.Empty;
    }
}
