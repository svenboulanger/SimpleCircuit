using System;

namespace SimpleCircuit.Components.Appearance
{
    /// <summary>
    /// A font size modifier.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="fontSize"></param>
    public class FontSizeAppearance(IAppearanceOptions parent, double fontSize) : IAppearanceOptions
    {
        private readonly IAppearanceOptions _parent = parent ?? throw new ArgumentNullException(nameof(parent));

        /// <inheritdoc />
        public string Color => _parent.Color;

        /// <inheritdoc />
        public double Opacity => _parent.Opacity;

        /// <inheritdoc />
        public string Background => _parent.Color;

        /// <inheritdoc />
        public double BackgroundOpacity => _parent.Opacity;

        /// <inheritdoc />
        public double LineThickness => _parent.LineThickness;

        /// <inheritdoc />
        public string FontFamily => _parent.FontFamily;

        /// <inheritdoc />
        public double FontSize => fontSize;

        /// <inheritdoc />
        public bool Bold => _parent.Bold;

        /// <inheritdoc />
        public double LineSpacing => _parent.LineSpacing;

        /// <inheritdoc />
        public int LineStyle => _parent.LineStyle;
    }
}
