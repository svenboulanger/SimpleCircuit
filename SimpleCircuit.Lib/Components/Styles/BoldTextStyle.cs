namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A style that boldens text.
    /// </summary>
    /// <param name="parent">The parent style.</param>
    public class BoldTextStyle(IStyle parent) : IStyle
    {
        private readonly IStyle _parent = parent;

        /// <inheritdoc />
        public string Color => _parent.Color;

        /// <inheritdoc />
        public double Opacity => _parent.Opacity;

        /// <inheritdoc />
        public string Background => _parent.Background;

        /// <inheritdoc />
        public double BackgroundOpacity => _parent.BackgroundOpacity;

        /// <inheritdoc />
        public double LineThickness => _parent.LineThickness;

        /// <inheritdoc />
        public string FontFamily => _parent.FontFamily;

        /// <inheritdoc />
        public double FontSize => _parent.FontSize;

        /// <inheritdoc />
        public bool Bold => true;

        /// <inheritdoc />
        public double LineSpacing => _parent.LineSpacing;

        /// <inheritdoc />
        public LineStyles LineStyle => _parent.LineStyle;
    }
}
