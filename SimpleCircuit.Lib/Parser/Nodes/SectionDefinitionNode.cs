using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A section definition.
    /// </summary>
    public record SectionDefinitionNode : SyntaxNode
    {
        /// <summary>
        /// Gets the SECTION token.
        /// </summary>
        public Token Section { get; }

        /// <summary>
        /// Gets the section name.
        /// </summary>
        public Token Name { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public SyntaxNode[] Properties { get; }

        /// <summary>
        /// Gets the statements inside the section.
        /// </summary>
        public ScopedStatementsNode Statements { get; }

        /// <summary>
        /// Creates a new <see cref="SectionDefinitionNode"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="statements">The statements.</param>
        public SectionDefinitionNode(Token section, Token name, IEnumerable<SyntaxNode> properties, ScopedStatementsNode statements)
            : base(section.Location)
        {
            Section = section;
            Name = name;
            Properties = properties?.ToArray() ?? [];
            Statements = statements ?? ScopedStatementsNode.Empty;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(".section ");
            sb.Append(Name.Content);
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
            sb.Append(".endsection");
            return sb.ToString();
        }
    }
}
