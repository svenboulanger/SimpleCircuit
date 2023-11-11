using System;

namespace SimpleCircuit
{
    /// <summary>
    /// An attribute that indicates an alias for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AliasAttribute : Attribute
    {
        /// <summary>
        /// Gets the alias.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Creates a new alias attribute.
        /// </summary>
        /// <param name="alias">The alias.</param>
        public AliasAttribute(string alias)
        {
            Alias = alias ?? string.Empty;
        }
    }
}
