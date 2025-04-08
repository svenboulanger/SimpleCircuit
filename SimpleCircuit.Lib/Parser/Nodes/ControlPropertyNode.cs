using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A node for setting the property for following components.
    /// </summary>
    public record ControlPropertyNode : SyntaxNode
    {
        /// <summary>
        /// Gets the key token.
        /// </summary>
        public Token Key { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public List<SyntaxNode> Properties { get; }

        /// <summary>
        /// Creates a new <see cref="ControlPropertyNode"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="properties">The properties.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ControlPropertyNode(Token key, IEnumerable<SyntaxNode> properties)
            : base(key.Location)
        {
            Key = key;
            Properties = properties?.ToList() ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{Key} {string.Join(" ", Properties)}";
    }
}
