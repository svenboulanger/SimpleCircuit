using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A scope definition.
    /// </summary>
    public record BoxNode : SyntaxNode
    {
        /// <summary>
        /// Gets the annotation token.
        /// </summary>
        public Token Annotation { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public SyntaxNode[] Properties { get; }

        /// <summary>
        /// Gets the statements.
        /// </summary>
        public ScopedStatementsNode Statements { get; }

        /// <summary>
        /// Creates a new <see cref="ScopeDefinitionNode"/>.
        /// </summary>
        /// <param name="scope">The scope keyword.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="statements">The statements.</param>
        public BoxNode(Token scope, IEnumerable<SyntaxNode> properties, ScopedStatementsNode statements)
            : base(scope.Location)
        {
            Annotation = scope;
            Properties = properties?.ToArray() ?? [];
            Statements = statements ?? ScopedStatementsNode.Empty;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append('.');
            sb.Append(Annotation.Content);
            if (Properties.Length > 0)
            {
                for (int i = 0; i < Properties.Length; i++)
                {
                    sb.Append(' ');
                    sb.Append(Properties[i]);
                }
            }
            sb.AppendLine();
            sb.AppendLine(Statements.ToString());
            sb.Append(".endb");
            return sb.ToString();
        }
    }
}
