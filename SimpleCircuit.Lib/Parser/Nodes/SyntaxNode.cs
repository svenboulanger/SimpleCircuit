namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// An abstract class describing a syntax node.
    /// </summary>
    public abstract record SyntaxNode
    {
        /// <summary>
        /// Gets the text location.
        /// </summary>
        public TextLocation Location { get; }

        /// <summary>
        /// Creates a new <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        protected SyntaxNode(TextLocation location)
        {
            Location = location;
        }
    }
}
