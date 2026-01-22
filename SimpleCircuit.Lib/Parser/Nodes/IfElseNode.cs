using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// An if-else node.
/// </summary>
public record IfElseNode : SyntaxNode
{
    /// <summary>
    /// Gets the conditions.
    /// </summary>
    public IfConditionNode[] Conditions { get; }

    /// <summary>
    /// Gets the statements if no condition matched.
    /// </summary>
    public ScopedStatementsNode Else { get; }

    /// <summary>
    /// Creates a new <see cref="IfElseNode"/>.
    /// </summary>
    /// <param name="ifLocation">The location of the .if-word.</param>
    /// <param name="conditions">The conditions.</param>
    /// <param name="ifTrueStatements">The statements for each condition.</param>
    /// <param name="elseStatements">The statements if no condition matched.</param>
    public IfElseNode(IEnumerable<IfConditionNode> conditions, ScopedStatementsNode elseNode)
        : base(conditions.FirstOrDefault()?.Location ?? default)
    {
        Conditions = conditions?.ToArray() ?? throw new ArgumentNullException(nameof(conditions));
        Else = elseNode;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();
        for (int i = 0; i < Conditions.Length; i++)
            sb.AppendLine(Conditions[i].ToString());
        if (Else is not null)
        {
            sb.AppendLine(".else");
            sb.AppendLine(Else.ToString());
        }
        sb.AppendLine(".endif");
        return sb.ToString();
    }
}
