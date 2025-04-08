using System;
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
        public SyntaxNode[] Statements { get; }

        /// <summary>
        /// Creates a new <see cref="SectionDefinitionNode"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="statements">The statements.</param>
        public SectionDefinitionNode(Token name, IEnumerable<SyntaxNode> properties, IEnumerable<SyntaxNode> statements)
            : base(name.Location)
        {
            Name = name;
            Properties = properties?.ToArray() ?? [];
            Statements = statements?.ToArray() ?? [];
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
            if (Statements.Length > 0)
            {
                sb.AppendLine();
                foreach (var statement in Statements)
                    sb.AppendLine(statement.ToString());
                sb.Append(".ends");
            }
            return sb.ToString();
        }
    }
}
