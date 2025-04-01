using System;

namespace SimpleCircuit.Parser.Nodes
{
    public enum BinaryOperatorTypes
    {
        None,
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Modulo,
        ShiftLeft,
        ShiftRight,
        GreaterThan,
        SmallerThan,
        GreaterThanOrEqual,
        SmallerThanOrEqual,
        Equals,
        NotEquals,
        And,
        Or,
        Xor,
        LogicalAnd,
        LogicalOr,
        Concatenate,
        Assignment,
    }

    /// <summary>
    /// A binary operation.
    /// </summary>
    public record BinaryNode : SyntaxNode
    {
        /// <summary>
        /// The operation type.
        /// </summary>
        public BinaryOperatorTypes Type { get; }

        /// <summary>
        /// The left argument.
        /// </summary>
        public SyntaxNode Left { get; }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        public Token Operator { get; }

        /// <summary>
        /// The right argument.
        /// </summary>
        public SyntaxNode Right { get; }

        /// <summary>
        /// Creates a new <see cref="BinaryNode"/>.
        /// </summary>
        /// <param name="type">The operator type.</param>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
        public BinaryNode(BinaryOperatorTypes type, SyntaxNode left, Token @operator, SyntaxNode right)
            : base(left.Location)
        {
            Type = type;
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Operator = @operator;
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{Left}{Operator.Content}{Right}";
    }
}
