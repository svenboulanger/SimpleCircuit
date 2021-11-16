using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Represents a text formatter.
    /// </summary>
    public interface IElementFormatter
    {
        /// <summary>
        /// Formats an XML element. The element can be changed if needed.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="element">The text element.</param>
        /// <returns>The computed bounds of the formatted element.</returns>
        public Bounds Format(SvgDrawing drawing, XmlElement element);
    }
}
