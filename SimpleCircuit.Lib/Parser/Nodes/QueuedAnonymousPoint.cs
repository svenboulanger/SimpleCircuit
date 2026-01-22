namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A queued anonymous point.
/// </summary>
public record QueuedAnonymousPoint : SyntaxNode
{
    /// <summary>
    /// Gets the token.
    /// </summary>
    public Token Token { get; }

    /// <summary>
    /// Creates a new <see cref="QueuedAnonymousPoint"/>.
    /// </summary>
    /// <param name="token">The token.</param>
    public QueuedAnonymousPoint(Token token)
        : base(token.Location)
    {
        Token = token;
    }

    /// <inheritdoc />
    public override string ToString() => $"Q[{Token.Content}]";
}
