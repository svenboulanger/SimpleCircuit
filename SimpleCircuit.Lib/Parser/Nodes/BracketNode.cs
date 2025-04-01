using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A bracket node.
    /// </summary>
    public record BracketNode : SyntaxNode
    {
        /// <summary>
        /// Gets the left bracket.
        /// </summary>
        public Token Left { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public SyntaxNode Value { get; }

        /// <summary>
        /// Gets the right bracket.
        /// </summary>
        public Token Right { get; }

        /// <summary>
        /// Creates a new <see cref="BracketNode"/>.
        /// </summary>
        /// <param name="left">The left bracket.</param>
        /// <param name="value">The value.</param>
        /// <param name="right">The right bracket.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BracketNode(Token left, SyntaxNode value, Token right)
            : base(left.Location)
        {
            Left = left;
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Right = right;
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{Left.Content}{Value}{Right.Content}";
    }
}
