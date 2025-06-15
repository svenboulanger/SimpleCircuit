namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A number.
    /// </summary>
    public record ConstantNode : SyntaxNode
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
        /// Creates a <see cref="ConstantNode"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="location">The location.</param>
        public ConstantNode(object value, Token content)
            : base(content.Location)
        {
            Content = content;
            Value = value;
        }

        /// <inheritdoc />
        public override string ToString() => Value.ToString();
    }
}
