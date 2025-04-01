namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A number.
    /// </summary>
    public record Number : SyntaxNode
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Creates a <see cref="Number"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="location">The location.</param>
        public Number(double value, TextLocation location)
            : base(location)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override string ToString() => Value.ToString();
    }
}
