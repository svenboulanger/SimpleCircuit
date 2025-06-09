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
        public string Name => Token.Content.ToString();

        /// <summary>
        /// Gets the token.
        /// </summary>
        public Token Token { get; }

        /// <summary>
        /// Creates a new <see cref="IdentifierNode"/>.
        /// </summary>
        /// <param name="token">The token.</param>
        public IdentifierNode(Token token)
            : base(token.Location)
        {
            Token = token;
        }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}
