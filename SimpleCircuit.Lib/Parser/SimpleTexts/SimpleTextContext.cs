using System;
using System.Text;
using System.Xml;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// A context for parsing SimpleCircuit text.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="SimpleTextContext"/>.
    /// </remarks>
    public class SimpleTextContext(ITextMeasurer measurer)
    {
        /// <summary>
        /// Gets the text measurer.
        /// </summary>
        public ITextMeasurer Measurer { get; } = measurer ?? new SkiaTextMeasurer();

        /// <summary>
        /// Gets the text builder.
        /// </summary>
        public StringBuilder Builder { get; } = new();

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public double FontSize { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the opacity.
        /// </summary>
        public double Opacity { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public string Color { get; set; } = "black";

        /// <summary>
        /// Gets or sets whether the text is bold.
        /// </summary>
        public bool IsBold { get; set; } = false;

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
    }
}
