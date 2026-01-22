using System;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// An include node.
/// </summary>
public record class IncludeNode : SyntaxNode
{
    /// <summary>
    /// Gets the include token.
    /// </summary>
    public Token IncludeToken { get; }

    /// <summary>
    /// Gets the filename.
    /// </summary>
    public SyntaxNode Filename { get; }

    /// <summary>
    /// Creates a new <see cref="IncludeNode"/>.
    /// </summary>
    /// <param name="includeToken">The include token.</param>
    /// <param name="filename">The filename.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="filename"/> is <c>null</c>.</exception>
    public IncludeNode(Token includeToken, SyntaxNode filename)
        : base(includeToken.Location)
    {
        IncludeToken = includeToken;
        Filename = filename ?? throw new ArgumentNullException(nameof(filename));
    }

    /// <inheritdoc />
    public override string ToString()
        => $".{IncludeToken} {Filename}";
}
