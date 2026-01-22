using System;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// Represents a component.
/// </summary>
public record class ComponentNode : SyntaxNode
{
    /// <summary>
    /// Gets the name of the component.
    /// </summary>
    public SyntaxNode Name { get; }

    /// <summary>
    /// The component.
    /// </summary>
    /// <param name="name">The name.</param>
    public ComponentNode(SyntaxNode name)
        : base(name.Location)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <inheritdoc />
    public override string ToString() => Name.ToString();
}
