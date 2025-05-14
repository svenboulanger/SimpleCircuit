using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// An <see cref="IStyle"/> that has no fill, and no line style.
    /// </summary>
    /// <param name="parent"></param>
    public class StrokeMarkerStyle(IStyle parent, double? lineThickness = null) : IStyle
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
        public double LineThickness => lineThickness ?? _parent.LineThickness;

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
