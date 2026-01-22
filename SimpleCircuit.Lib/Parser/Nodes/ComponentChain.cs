using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A component chain.
/// </summary>
public record ComponentChain : SyntaxNode
{
    /// <summary>
    /// Gets the items in the chain.
    /// </summary>
    public SyntaxNode[] Items { get; }

    /// <summary>
    /// Creates a new <see cref="ComponentChain"/>.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="items"/> is <c>null</c>.</exception>
    public ComponentChain(IEnumerable<SyntaxNode> items)
        : base(items?.First().Location ?? default)
    {
        Items = items?.ToArray() ?? throw new ArgumentNullException(nameof(items));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();
        for (int i = 0; i < Items.Length; i++)
        {
            if (i > 0)
                sb.Append(' ');
            sb.Append(Items[i]);
        }
        return sb.ToString();
    }
}
