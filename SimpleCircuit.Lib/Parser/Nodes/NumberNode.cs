namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A number.
    /// </summary>
    public record NumberNode : SyntaxNode
    {
        /// <summary>
        /// Gets the token that represents the number.
        /// </summary>
        public Token Content { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Creates a <see cref="NumberNode"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="location">The location.</param>
        public NumberNode(object value, TextLocation location)
            : base(location)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override string ToString() => Value.ToString();
    }
}
