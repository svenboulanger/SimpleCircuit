using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// An identifier.
    /// </summary>
    public record IdentifierNode : SyntaxNode
    {
        /// <summary>
        /// Gets the identifier
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new <see cref="IdentifierNode"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="location">The location.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public IdentifierNode(string name, TextLocation location)
            : base(location)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Creates a new <see cref="IdentifierNode"/>.
        /// </summary>
        /// <param name="token">The token.</param>
        public IdentifierNode(Token token)
            : base(token.Location)
        {
            Name = token.Content.ToString();
        }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}
