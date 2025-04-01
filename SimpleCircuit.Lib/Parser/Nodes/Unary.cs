using System;

namespace SimpleCircuit.Parser.Nodes
{
    public enum UnaryOperatorTypes
    {
        None,
        Positive,
        Negative,
        Invert,
    }

    /// <summary>
    /// A unary operation.
    /// </summary>
    public record Unary : SyntaxNode
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        public UnaryOperatorTypes Type { get; }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        public Token Operator { get; }

        /// <summary>
        /// Gets the argument.
        /// </summary>
        public SyntaxNode Argument { get; }
        
        /// <summary>
        /// Creates a new <see cref="Unary"/>.
        /// </summary>
        /// <param name="operator">The operator</param>
        /// <param name="arg">The argument.</param>
        /// <param name="type">The operator type.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="arg"/> is <c>null</c>.</exception>
        public Unary(Token @operator, SyntaxNode arg, UnaryOperatorTypes type)
            : base(@operator.Location)
        {
            Operator = @operator;
            Type = type;
            Argument = arg ?? throw new ArgumentNullException(nameof(arg));
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{Operator.Content}{Argument}";
    }
}
