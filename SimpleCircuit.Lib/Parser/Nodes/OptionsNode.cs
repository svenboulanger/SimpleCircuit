using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// An options statement.
/// </summary>
public record OptionsNode : SyntaxNode
{
    /// <summary>
    /// Gets the options keyword.
    /// </summary>
    public Token Options { get; }

    /// <summary>
    /// Gets the properties.
    /// </summary>
    public SyntaxNode[] Properties { get; }

    /// <summary>
    /// Creates a new <see cref="OptionsNode"/>.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="properties">The properties.</param>
    public OptionsNode(Token options, IEnumerable<SyntaxNode> properties)
        : base(options.Location)
    {
        Options = options;
        Properties = properties?.ToArray() ?? [];
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($".{Options.Content}");
        foreach (var property in Properties)
        {
            sb.Append(' ');
            sb.Append(property.ToString());
        }
        return sb.ToString();
    }
}
