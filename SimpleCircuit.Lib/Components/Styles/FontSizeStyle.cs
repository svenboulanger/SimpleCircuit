using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A style that fixes text size and line thickness. It applies the following style modifications:
    /// the line thickness is fixed for over- and underlines, the font size overridden.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="fontSize">The font size.</param>
    public class FontSizeStyle(IStyle parent, double fontSize = Style.DefaultFontSize) : IStyle
    {
        private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));

        /// <inheritdoc />
        public string Color => _parent.Color;

        /// <inheritdoc />
        public double Opacity => _parent.Opacity;

        /// <inheritdoc />
        public string Background => _parent.Color;

        /// <inheritdoc />
        public double BackgroundOpacity => _parent.Opacity;

        /// <inheritdoc />
        public double LineThickness => 0.1 * FontSize;

        /// <inheritdoc />
        public string FontFamily => _parent.FontFamily;

        /// <inheritdoc />
        public double FontSize => fontSize;

        /// <inheritdoc />
        public bool Bold => _parent.Bold;

        /// <inheritdoc />
        public double LineSpacing => _parent.LineSpacing;

        /// <inheritdoc />
        public LineStyles LineStyle => _parent.LineStyle;
    }
}
