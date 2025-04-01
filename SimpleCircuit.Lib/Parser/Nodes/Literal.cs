using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A literal value.
    /// </summary>
    public record Literal : SyntaxNode
    {
        /// <summary>
        /// Gets the literal value.
        /// </summary>
        public ReadOnlyMemory<char> Value { get; }

        /// <summary>
        /// Creates a new <see cref="Literal"/>.
        /// </summary>
        /// <param name="token">The token.</param>
        public Literal(Token token)
            : base(token.Location)
        {
            Value = token.Content;
        }

        /// <inheritdoc />
        public override string ToString() => Value.ToString();
    }
}
