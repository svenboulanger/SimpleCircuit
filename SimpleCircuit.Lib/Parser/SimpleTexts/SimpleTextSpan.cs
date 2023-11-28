using SimpleCircuit.Drawing;
using System.Xml;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// Represents a span of text with its own relative position and bounds.
    /// </summary>
    public readonly struct SimpleTextSpan
    {
        /// <summary>
        /// Gets the text content of the text span.
        /// </summary>
        public XmlElement Element { get; }

        /// <summary>
        /// Gets the offset of the origin compared to that of the line.
        /// </summary>
        public Vector2 Delta { get; }

        /// <summary>
        /// Gets the bounds of the text span, assuming it would be drawn at (0, 0).
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// Creates a new <see cref="SimpleTextSpan"/>
        /// </summary>
        /// <param name="element">The content.</param>
        /// <param name="delta">The origin offset.</param>
        /// <param name="bounds">The bounds.</param>
        public SimpleTextSpan(XmlElement element, Vector2 delta, Bounds bounds)
        {
            Element = element;
            Delta = delta;
            Bounds = bounds;
        }
    }
}
