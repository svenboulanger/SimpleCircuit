using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// A simple text formatter.
    /// </summary>
    public class ElementFormatter : IElementFormatter
    {
        /// <summary>
        /// Represents a width for a lowercase character relative to the font size.
        /// </summary>
        public double LowerCharacterWidth { get; set; } = 1.0;

        /// <summary>
        /// Represents a width for an uppercase character relative to the font size..
        /// </summary>
        public double UpperCharacterWidth { get; set; } = 1.25;

        /// <summary>
        /// Gets the middle-line factor.
        /// </summary>
        public double MidLineFactor { get; set; } = 0.125;

        /// <summary>
        /// Gets the font size.
        /// </summary>
        public double Size { get; set; } = 4.0;

        /// <inheritdoc />
        public Bounds Format(SvgDrawing drawing, XmlElement element)
        {
            double w = 0;
            foreach (var c in element.InnerText)
                w += char.IsLower(c) ? LowerCharacterWidth : UpperCharacterWidth;
            return new(0, -Size * (1 - MidLineFactor), w * Size, Size * MidLineFactor);
        }
    }
}
