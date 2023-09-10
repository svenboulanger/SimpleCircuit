using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Represents the information for a drawable.
    /// </summary>
    public interface IDrawableInfo
    {
        /// <summary>
        /// Gets the token that describes the component name.
        /// </summary>
        public Token Source { get; }

        /// <summary>
        /// Gets the full name of the drawable.
        /// </summary>
        public string Fullname { get; }

        /// <summary>
        /// Gets the labels specified for the drawable.
        /// </summary>
        public IList<Token> Labels { get; }

        /// <summary>
        /// Gets the variants specified for the drawable.
        /// </summary>
        public IList<VariantInfo> Variants { get; }

        /// <summary>
        /// Gets the properties for the component.
        /// </summary>
        public IDictionary<Token, object> Properties { get; }
    }
}
