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
        /// <param name="element">The element to be measured.</param>
        /// <returns>The computed bounds of the formatted element.</returns>
        public Bounds Measure(XmlElement element);
    }
}
