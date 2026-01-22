using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes;

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
    public SyntaxNode Name { get; }

    /// <summary>
    /// Gets the section template.
    /// </summary>
    public SyntaxNode Template { get; }

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
    /// <param name="section">The section token.</param>
    /// <param name="name">The name.</param>
    /// <param name="template">An optional template token.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="statements">The statements.</param>
    public SectionDefinitionNode(Token section, SyntaxNode name, SyntaxNode template, IEnumerable<SyntaxNode> properties, ScopedStatementsNode statements)
        : base(section.Location)
    {
        Section = section;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Template = template;
        Properties = properties?.ToArray() ?? [];
        Statements = statements ?? ScopedStatementsNode.Empty;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(".section ");
        sb.Append(Name);
        if (Template is not null)
        {
            sb.Append(' ');
            sb.Append(Template);
            if (Properties.Length > 0)
            {
                sb.Append('(');
                for (int i = 0; i < Properties.Length; i++)
                {
                    if (i > 0)
                        sb.Append(' ');
                    sb.Append(Properties[i]);
                }
                sb.Append(')');
            }
        }
        else
        {
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
        }
        return sb.ToString();
    }
}
