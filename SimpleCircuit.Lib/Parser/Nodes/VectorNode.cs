using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A vector.
/// </summary>
public record VectorNode : SyntaxNode, IEquatable<VectorNode>
{
    /// <summary>
    /// Gets the arguments of the vector.
    /// </summary>
    public SyntaxNode[] Arguments { get; }

    /// <summary>
    /// Creates a new <see cref="VectorNode"/>.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    public VectorNode(IEnumerable<SyntaxNode> arguments)
        : base(arguments?.First() ?? default)
    {
        Arguments = arguments?.ToArray() ?? [];
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        int hash = 0;
        for (int i = 0; i < Arguments.Length; i++)
            hash = (hash * 1023) ^ Arguments[i].GetHashCode();
        return hash;
    }

    /// <inheritdoc />
    public virtual bool Equals(VectorNode other)
    {
        if (Arguments.Length != other.Arguments.Length)
            return false;
        for (int i = 0; i < Arguments.Length; i++)
        {
            if (!Arguments[i].Equals(other.Arguments[i]))
                return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
        => string.Join(", ", Arguments.Select(arg => arg.ToString()));
}
