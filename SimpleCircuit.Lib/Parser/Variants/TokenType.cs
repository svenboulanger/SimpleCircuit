namespace SimpleCircuit.Parser.Variants;

/// <summary>
/// The token type for variants.
/// </summary>
public enum TokenType
{
    /// <summary>
    /// End of the content.
    /// </summary>
    EndOfContent = 0,

    /// <summary>
    /// A variant name.
    /// </summary>
    Variant = 0x01,

    /// <summary>
    /// An AND binary operator.
    /// </summary>
    And = 0x02,

    /// <summary>
    /// An OR binary operator.
    /// </summary>
    Or = 0x04,

    /// <summary>
    /// A unary operator.
    /// </summary>
    Not = 0x08,

    /// <summary>
    /// An opening bracket.
    /// </summary>
    OpenBracket = 0x10,

    /// <summary>
    /// A closing bracket.
    /// </summary>
    CloseBracket = 0x20,

    /// <summary>
    /// All token type.
    /// </summary>
    All = -1
}
