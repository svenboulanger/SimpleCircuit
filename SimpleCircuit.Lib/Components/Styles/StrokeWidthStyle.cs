using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// Represents a style based on a parent style that overrides the stroke width.
    /// </summary>
    /// <param name="parent">The parent style.</param>
    /// <param name="lineThickness">The stroke width.</param>
    public class StrokeWidthStyle(IStyle parent, double lineThickness = Style.DefaultLineThickness) : IStyle
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
        public double LineThickness => lineThickness;

        /// <inheritdoc />
        public string FontFamily => _parent.FontFamily;

        /// <inheritdoc />
        public double FontSize => _parent.FontSize;

        /// <inheritdoc />
        public bool Bold => _parent.Bold;

        /// <inheritdoc />
        public double LineSpacing => _parent.LineSpacing;

        /// <inheritdoc />
        public LineStyles LineStyle => _parent.LineStyle;
    }
}