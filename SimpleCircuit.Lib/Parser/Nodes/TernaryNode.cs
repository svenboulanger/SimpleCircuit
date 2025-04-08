using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A ternary operator.
    /// </summary>
    public record TernaryNode : SyntaxNode
    {
        /// <summary>
        /// Gets the left argument.
        /// </summary>
        public SyntaxNode Left { get; }

        /// <summary>
        /// Gets the middle argument.
        /// </summary>
        public SyntaxNode Middle { get; }

        /// <summary>
        /// Gets the right argument.
        /// </summary>
        public SyntaxNode Right { get; }

        /// <summary>
        /// Creates a new <see cref="TernaryNode"/>
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="middle">The middle argument.</param>
        /// <param name="right">The right argument.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="left"/>, <paramref name="middle"/> or <paramref name="right"/> is <c>null</c>.</exception>
        public TernaryNode(SyntaxNode left, SyntaxNode middle, SyntaxNode right)
            : base(left.Location)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Middle = middle ?? throw new ArgumentNullException(nameof(middle));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        /// <inheritdoc />
        public override string ToString()
            => $"({Left}?{Middle}:{Right})";
    }
}
