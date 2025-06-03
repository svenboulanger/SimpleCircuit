namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// Describes the appearance of an item.
    /// </summary>
    public interface IStyle
    {
        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        public string Color { get; }

        /// <summary>
        /// Gets or sets the foreground opacity.
        /// </summary>
        public double Opacity { get; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public string Background { get; }

        /// <summary>
        /// Gets or sets the background opacity.
        /// </summary>
        public double BackgroundOpacity { get; }

        /// <summary>
        /// Gets or sets the stroke thickness.
        /// </summary>
        public double LineThickness { get; }

        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        public string FontFamily { get; }

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public double FontSize { get; }

        /// <summary>
        /// Gets or sets whether text should be bold.
        /// </summary>
        public bool Bold { get; }

        /// <summary>
        /// Gets or sets the line spacing.
        /// </summary>
        public double LineSpacing { get; }

        /// <summary>
        /// Gets the text justification.
        /// </summary>
        public double Justification { get; }

        /// <summary>
        /// Gets or sets the line style.
        /// </summary>
        public string StrokeDashArray { get; }
    }
}
