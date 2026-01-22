using System;
using System.Text;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A for-loop.
/// </summary>
public record ForLoopNode : SyntaxNode
{
    /// <summary>
    /// Gets the FOR token.
    /// </summary>
    public Token For { get; }

    /// <summary>
    /// Gets the variable name.
    /// </summary>
    public Token Variable { get; }

    /// <summary>
    /// Gets the start value.
    /// </summary>
    public SyntaxNode Start { get; }

    /// <summary>
    /// Gets the end value.
    /// </summary>
    public SyntaxNode End { get; }

    /// <summary>
    /// Gets the increment value.
    /// </summary>
    public SyntaxNode Increment { get; }

    /// <summary>
    /// Gets the statements in the for-loop.
    /// </summary>
    public ScopedStatementsNode Statements { get; }

    /// <summary>
    /// Creates a new <see cref="ForLoopNode"/>.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <param name="start">The start value.</param>
    /// <param name="end">The end value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="statements">The statements.</param>
    public ForLoopNode(Token forToken, Token variable, SyntaxNode start, SyntaxNode end, SyntaxNode increment, ScopedStatementsNode statements)
        : base(forToken.Location)
    {
        For = forToken;
        Variable = variable;
        Start = start ?? throw new ArgumentNullException(nameof(start));
        End = end ?? throw new ArgumentNullException(nameof(end));
        Increment = increment ?? throw new ArgumentNullException(nameof(increment));
        Statements = statements ?? ScopedStatementsNode.Empty;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append('.');
        sb.Append(For.Content);
        sb.Append(' ');
        sb.Append(Variable.Content);
        sb.Append(' ');
        sb.Append(Start);
        sb.Append(' ');
        sb.Append(End);
        sb.Append(' ');
        sb.Append(Increment);
        sb.AppendLine();
        sb.AppendLine(Statements.ToString());
        sb.Append(".endfor");
        return sb.ToString();
    }
}
