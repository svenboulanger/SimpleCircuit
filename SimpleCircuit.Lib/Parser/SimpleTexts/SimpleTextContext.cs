using System;
using System.Text;
using System.Xml;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// A context for parsing SimpleCircuit text.
    /// </summary>
    public class SimpleTextContext
    {
        private XmlNode _text = null;

        /// <summary>
        /// Gets the document.
        /// </summary>
        public XmlDocument Document { get; }

        /// <summary>
        /// Gets the parent node.
        /// </summary>
        public XmlNode Parent { get; }

        /// <summary>
        /// Gets the current line.
        /// </summary>
        public XmlNode Text { get; set; }

        /// <summary>
        /// Gets the text measurer.
        /// </summary>
        public ITextMeasurer Measurer { get; }

        /// <summary>
        /// Gets the text builder.
        /// </summary>
        public StringBuilder Builder { get; } = new();

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public double FontSize { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the line spacing.
        /// </summary>
        public double LineSpacing { get; set; } = 1.5;

        /// <summary>
        /// Gets or sets the expansion direction.
        /// </summary>
        public Vector2 Expand { get; set; }

        /// <summary>
        /// Gets the alignment for multiple lines.
        /// </summary>
        public double Align { get; set; }

        /// <summary>
        /// Creates a new <see cref="SimpleTextContext"/>.
        /// </summary>
        public SimpleTextContext(XmlNode parent, ITextMeasurer measurer)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Document = parent.OwnerDocument;
            Measurer = measurer ?? new SkiaTextMeasurer();
        }
    }
}
