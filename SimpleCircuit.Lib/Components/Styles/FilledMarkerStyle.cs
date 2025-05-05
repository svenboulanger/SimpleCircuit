using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// An <see cref="IStyle"/> that fills up any shapes with the foreground color. It combines the following modifications:
    /// the shape will have a fill, stroke and opacity as the foreground color and the line style is removed.
    /// </summary>
    /// <param name="parent">The parent style.</param>
    public class FilledMarkerStyle(IStyle parent) : IStyle
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
