using System;

namespace SimpleCircuit.Drawing.Builders;

/// <summary>
/// Enumeration of text orientation type.
/// </summary>
[Flags]
public enum TextOrientationType
{
    /// <summary>
    /// Nothing special about the orientation type. Just left-to-right text independent of the current transform.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that the text is transformed.
    /// </summary>
    Transformed = 0x02
}
