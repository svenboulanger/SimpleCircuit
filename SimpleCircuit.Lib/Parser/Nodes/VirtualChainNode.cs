using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A virtual chain.
/// </summary>
public record VirtualChainNode : SyntaxNode
{
    /// <summary>
    /// Gets the left bracket.
    /// </summary>
    public Token Left { get; }

    /// <summary>
    /// Gets the constraints.
    /// </summary>
    public Token? Constraints { get; }

    /// <summary>
    /// Gets the items in the virtual chain.
    /// </summary>
    public SyntaxNode[] Items { get; }

    /// <summary>
    /// Gets the constraints for the virtual chain.
    /// </summary>
    public VirtualChainConstraints Flags { get; }

    /// <summary>
    /// Gets the right bracket.
    /// </summary>
    public Token Right { get; } 

    /// <summary>
    /// Creates a new <see cref="VirtualChainNode"/>.
    /// </summary>
    /// <param name="left">The left bracket</param>
    /// <param name="constraints">The constraints specifier.</param>
    /// <param name="items">The items.</param>
    /// <param name="right">The right bracket.</param>
    /// <param name="constraints">The constraints.</param>
    public VirtualChainNode(Token left, Token? constraints, IEnumerable<SyntaxNode> items, Token right, VirtualChainConstraints flags)
        : base(left.Location)
    {
        Left = left;
        Constraints = constraints;
        Items = items?.ToArray() ?? throw new ArgumentNullException(nameof(items));
        Right = right;
        Flags = flags;
    }

    /// <inheritdoc />
    public override string ToString()
        => Constraints is null ? $"{Left.Content}{string.Join(" ", (object[])Items)}{Right.Content}" : $"{Left.Content}{Constraints.Value.Content} {string.Join(" ", (object[])Items)}{Right.Content}";
}
