using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A node for setting the property for following components.
/// </summary>
public record ControlPropertyNode : SyntaxNode
{
    /// <summary>
    /// Gets the filter node.
    /// </summary>
    public SyntaxNode Filter { get; }

    /// <summary>
    /// Gets the properties.
    /// </summary>
    public List<SyntaxNode> Properties { get; }

    /// <summary>
    /// Creates a new <see cref="ControlPropertyNode"/>.
    /// </summary>
    /// <param name="name">The key.</param>
    /// <param name="properties">The properties.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ControlPropertyNode(SyntaxNode name, IEnumerable<SyntaxNode> properties)
        : base(name.Location)
    {
        Filter = name;
        Properties = properties?.ToList() ?? throw new ArgumentNullException(nameof(properties));
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{Filter} {string.Join(" ", Properties)}";
}
