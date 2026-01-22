using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A node that represents a .theme statement.
/// </summary>
public record class ThemeNode : SyntaxNode
{
    /// <summary>
    /// Gets the 'theme' keyword.
    /// </summary>
    public Token Theme { get; }

    /// <summary>
    /// Gets the name of the theme.
    /// </summary>
    public SyntaxNode Name { get; }

    /// <summary>
    /// Gets the properties of the theme.
    /// </summary>
    public SyntaxNode[] Properties { get; }

    /// <summary>
    /// Creates a new <see cref="ThemeNode"/>.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="name">The name.</param>
    /// <param name="properties">The properties.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c></exception>
    public ThemeNode(Token theme, SyntaxNode name, IEnumerable<SyntaxNode> properties)
        : base(theme.Location)
    {
        Theme = theme;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Properties = properties?.ToArray() ?? [];
    }

    /// <inheritdoc />
    public override string ToString()
        => $".{Theme.Content} {Name} {string.Join(" ", Properties.Select(p => p.ToString()))}";
}
