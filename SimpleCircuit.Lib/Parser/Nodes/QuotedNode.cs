using System;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A quoted value.
/// </summary>
public record QuotedNode : SyntaxNode
{
    /// <summary>
    /// Gets the unquoted value.
    /// </summary>
    public ReadOnlyMemory<char> Value { get; }

    /// <summary>
    /// Creates a new <see cref="QuotedNode"/>.
    /// </summary>
    /// <param name="token">The token.</param>
    public QuotedNode(Token token)
        : base(token.Location)
    {
        Value = token.Content[1..^1];
    }

    /// <inheritdoc />
    public override string ToString() => $"\"{Value}\"";
}
