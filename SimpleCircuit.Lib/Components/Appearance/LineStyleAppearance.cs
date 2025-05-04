using System;

namespace SimpleCircuit.Components.Appearance
{
    /// <summary>
    /// An appearance that only changes the line style.
    /// </summary>
    /// <param name="parent">The parent appearance.</param>
    /// <param name="lineStyle">The line style.</param>
    public class LineStyleAppearance(IAppearanceOptions parent, int lineStyle) : IAppearanceOptions
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
        public double FontSize => _parent.FontSize;

        /// <inheritdoc />
        public bool Bold => _parent.Bold;

        /// <inheritdoc />
        public double LineSpacing => _parent.LineSpacing;

        /// <inheritdoc />
        public int LineStyle => lineStyle;
    }
}
