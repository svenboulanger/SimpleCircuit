using System;

namespace SimpleCircuit
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DescriptionAttribute : Attribute
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new description attribute.
        /// </summary>
        /// <param name="description">The description.</param>
        public DescriptionAttribute(string description)
        {
            Description = description ?? "";
        }
    }
}
