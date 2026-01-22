using System;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A unary operation.
/// </summary>
public record UnaryNode : SyntaxNode
{
    /// <summary>
    /// Gets the type.
    /// </summary>
    public UnaryOperatortype Type { get; }

    /// <summary>
    /// Gets the operator.
    /// </summary>
    public Token Operator { get; }

    /// <summary>
    /// Gets the argument.
    /// </summary>
    public SyntaxNode Argument { get; }
    
    /// <summary>
    /// Creates a new <see cref="UnaryNode"/>.
    /// </summary>
    /// <param name="operator">The operator</param>
    /// <param name="arg">The argument.</param>
    /// <param name="type">The operator type.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="arg"/> is <c>null</c>.</exception>
    public UnaryNode(Token @operator, SyntaxNode arg, UnaryOperatortype type)
        : base(@operator.Location)
    {
        Operator = @operator;
        Type = type;
        Argument = arg ?? throw new ArgumentNullException(nameof(arg));
    }

    /// <inheritdoc />
    public override string ToString()
        => Type switch
        {
            UnaryOperatortype.PostfixDecrement or UnaryOperatortype.PostfixIncrement => $"{Argument}{Operator.Content}",
            _ => $"{Operator.Content}{Argument}"
        };
}
