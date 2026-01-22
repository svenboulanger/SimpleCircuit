using SimpleCircuit.Drawing;
using System.Xml;

namespace SimpleCircuit.Parser.SimpleTexts;

/// <summary>
/// Represents a span of text with its own relative position and bounds.
/// </summary>
/// <remarks>
/// Creates a new <see cref="SimpleTextSpan"/>
/// </remarks>
/// <param name="element">The content.</param>
/// <param name="delta">The origin offset.</param>
/// <param name="bounds">The bounds.</param>
public readonly struct SimpleTextSpan(XmlElement element, Vector2 delta, Bounds bounds)
{
    /// <summary>
    /// Gets the text content of the text span.
    /// </summary>
    public XmlElement Element { get; } = element;

    /// <summary>
    /// Gets the offset of the origin compared to that of the line.
    /// </summary>
    public Vector2 Delta { get; } = delta;

    /// <summary>
    /// Gets the bounds of the text span, assuming it would be drawn at (0, 0).
    /// </summary>
    public Bounds Bounds { get; } = bounds;
}
