using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A quoted value.
    /// </summary>
    public record Quoted : SyntaxNode
    {
        /// <summary>
        /// Gets the unquoted value.
        /// </summary>
        public ReadOnlyMemory<char> Value { get; }

        /// <summary>
        /// Creates a new <see cref="Quoted"/>.
        /// </summary>
        /// <param name="token">The token.</param>
        public Quoted(Token token)
            : base(token.Location)
        {
            Value = token.Content[1..^1];
        }

        /// <inheritdoc />
        public override string ToString() => $"\"{Value}\"";
    }
}
