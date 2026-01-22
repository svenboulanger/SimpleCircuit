using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// Statements with their own scope for parameter definitions.
/// </summary>
public record ScopedStatementsNode : SyntaxNode
{
    /// <summary>
    /// Gets an empty scoped statements node.
    /// </summary>
    public static ScopedStatementsNode Empty { get; } = new([], [], [], []);

    /// <summary>
    /// Gets the statements.
    /// </summary>
    public SyntaxNode[] Statements { get; }

    /// <summary>
    /// Gets the parameter definitions.
    /// </summary>
    public ParameterDefinitionNode[] ParameterDefinitions { get; }

    /// <summary>
    /// Gets the default variants and properties.
    /// </summary>
    public SyntaxNode[] ControlStatements { get; }

    /// <summary>
    /// Gets a sorted array of strings that contain all the references.
    /// </summary>
    /// <remarks>
    /// Rather than a set, we can 
    /// </remarks>
    public string[] References { get; }

    /// <summary>
    /// Creates a new <see cref="ScopedStatementsNode"/>
    /// </summary>
    /// <param name="statements">The statements.</param>
    /// <param name="parameterDefinitions">The parameter definitions.</param>
    /// <param name="references">The references.</param>
    public ScopedStatementsNode(IEnumerable<SyntaxNode> statements, IEnumerable<ParameterDefinitionNode> parameterDefinitions, IEnumerable<SyntaxNode> controlStatements, IEnumerable<string> references)
        : base(statements.FirstOrDefault()?.Location ?? default)
    {
        Statements = statements?.ToArray() ?? [];
        ParameterDefinitions = parameterDefinitions?.ToArray() ?? [];
        ControlStatements = controlStatements?.ToArray() ?? [];
        References = references?.ToArray() ?? [];
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();
        foreach (var statement in Statements)
        {
            if (sb.Length > 0)
                sb.AppendLine();
            sb.Append(statement.ToString());
        }
        foreach (var parameterDefinition in ParameterDefinitions)
        {
            if (sb.Length > 0)
                sb.AppendLine();
            sb.Append(parameterDefinition.ToString());
        }
        foreach (var statement in ControlStatements)
        {
            if (sb.Length > 0)
                sb.AppendLine();
            sb.Append(statement.ToString());
        }
        return sb.ToString();
    }
}
