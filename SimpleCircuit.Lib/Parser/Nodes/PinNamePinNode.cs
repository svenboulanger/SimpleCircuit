using System;

namespace SimpleCircuit.Parser.Nodes;

/// <summary>
/// A name with a pin before and after.
/// </summary>
public record PinNamePinNode : SyntaxNode
{
    /// <summary>
    /// Gets the left pin.
    /// </summary>
    public SyntaxNode PinLeft { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public SyntaxNode Name { get; }

    /// <summary>
    /// Gets the right pin.
    /// </summary>
    public SyntaxNode PinRight { get; }

    /// <summary>
    /// Creates a new <see cref="PinNamePinNode"/>.
    /// </summary>
    /// <param name="left">The left pin.</param>
    /// <param name="name">The name.</param>
    /// <param name="right">The right pin.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
    public PinNamePinNode(SyntaxNode left, SyntaxNode name, SyntaxNode right)
        : base(left?.Location ?? name?.Location ?? default)
    {
        PinLeft = left;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PinRight = right;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (PinLeft is not null && PinRight is not null)
            return $"[{PinLeft}]{Name}[{PinRight}]";
        else if (PinLeft is not null)
            return $"[{PinLeft}]{Name}";
        else if (PinRight is not null)
            return $"{Name}[{PinRight}]";
        else
            return Name.ToString();
    }
}
