using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A style that will override styling to remove any background color.
    /// </summary>
    /// <param name="parent">The parent appearance.</param>
    public class NoFillStyle(IStyle parent) : IStyle
    {
        private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));

        /// <inheritdoc />
        public string Color => _parent.Color;

        /// <inheritdoc />
        public double Opacity => _parent.Opacity;

        /// <inheritdoc />
        public string Background => Style.None;

        /// <inheritdoc />
        public double BackgroundOpacity => Style.Opaque;

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
        public LineStyles LineStyle => LineStyles.None;
    }
}
