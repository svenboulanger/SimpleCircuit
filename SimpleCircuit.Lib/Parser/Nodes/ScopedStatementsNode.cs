using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// Statements with their own scope for parameter definitions.
    /// </summary>
    public record ScopedStatementsNode : SyntaxNode
    {
        /// <summary>
        /// Gets an empty scoped statements node.
        /// </summary>
        public static ScopedStatementsNode Empty { get; } = new([], []);

        /// <summary>
        /// Gets the statements.
        /// </summary>
        public SyntaxNode[] Statements { get; }

        /// <summary>
        /// Gets the parameter definitions.
        /// </summary>
        public ParameterDefinitionNode[] ParameterDefinitions { get; }

        /// <summary>
        /// Creates a new <see cref="ScopedStatementsNode"/>
        /// </summary>
        /// <param name="statements">The statements.</param>
        /// <param name="parameterDefinitions">The parameter definitions.</param>
        public ScopedStatementsNode(IEnumerable<SyntaxNode> statements, IEnumerable<ParameterDefinitionNode> parameterDefinitions)
            : base(statements.FirstOrDefault()?.Location ?? parameterDefinitions.First().Location)
        {
            Statements = statements?.ToArray() ?? [];
            ParameterDefinitions = parameterDefinitions?.ToArray() ?? [];
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
            return sb.ToString();
        }
    }
}
