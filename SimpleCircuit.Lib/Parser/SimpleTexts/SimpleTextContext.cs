using SimpleCircuit.Components.Appearance;
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
        /// Gets or sets the current appearance.
        /// </summary>
        public IAppearanceOptions Appearance { get; set; }

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
