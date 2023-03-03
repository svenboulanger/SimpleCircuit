using SimpleCircuit.Diagnostics;
using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Represents a text formatter.
    /// </summary>
    public interface ITextFormatter
    {
        /// <summary>
        /// Measures the bounds of an XML element.
        /// </summary>
        /// <param name="element">The element to be measured.</param>
        /// <returns>The computed bounds of the formatted element.</returns>
        public Bounds Measure(XmlElement element);

        /// <summary>
        /// Formats text and adds it as children to the parent element.
        /// </summary>
        /// <param name="parent">The parent node of the formatted text.</param>
        /// <param name="value">The text element.</param>
        /// <param name="location">The location of the text.</param>
        /// <param name="expand">The direction that the text can overflow to.</param>
        /// <param name="options">The graphic options of the text.</param>
        /// <param name="diagnostics">The diagnostic handler.</param>
        /// <returns>
        ///     Returns the text element that contains the formatted text.
        /// </returns>
        public Bounds Format(XmlNode parent, string value, Vector2 location, Vector2 expand, GraphicOptions options, IDiagnosticHandler diagnostics);
    }
}
