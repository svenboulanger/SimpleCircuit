using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A literal value.
    /// </summary>
    public record LiteralNode : SyntaxNode
    {
        /// <summary>
        /// Gets the literal value.
        /// </summary>
        public ReadOnlyMemory<char> Value { get; }

        /// <summary>
        /// Creates a new <see cref="LiteralNode"/>.
        /// </summary>
        /// <param name="token">The token.</param>
        public LiteralNode(Token token)
            : base(token.Location)
        {
            Value = token.Content;
        }

        /// <inheritdoc />
        public override string ToString() => Value.ToString();
    }
}
