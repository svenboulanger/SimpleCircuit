namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// Describes a style.
    /// </summary>
    public class Style : IStyle
    {
        /// <summary>
        /// The default color.
        /// </summary>
        public const string DefaultColor = "black";

        /// <summary>
        /// An identifier representing no color.
        /// </summary>
        public const string None = "none";

        /// <summary>
        /// Represents an opaque opacity.
        /// </summary>
        public const double Opaque = 1.0;

        /// <summary>
        /// The default line thickness.
        /// </summary>
        public const double DefaultLineThickness = 0.5;

        /// <summary>
        /// The default font family.
        /// </summary>
        public const string DefaultFontFamily = "Arial";

        /// <summary>
        /// The default font size.
        /// </summary>
        public const double DefaultFontSize = 4.0;

        /// <summary>
        /// the default line spacing.
        /// </summary>
        public const double DefaultLineSpacing = 1.5;

        /// <summary>
        /// The default line style.
        /// </summary>
        public const LineStyles DefaultLineStyle = LineStyles.None;

        /// <inheritdoc />
        public string Color { get; set; } = DefaultColor;

        /// <inheritdoc />
        public double Opacity { get; set; } = Opaque;

        /// <inheritdoc />
        public string Background { get; set; } = None;

        /// <inheritdoc />
        public double BackgroundOpacity { get; set; } = Opaque;

        /// <inheritdoc />
        public double LineThickness { get; set; } = DefaultLineThickness;

        /// <inheritdoc />
        public string FontFamily { get; set; } = DefaultFontFamily;

        /// <inheritdoc />
        public double FontSize { get; set; } = DefaultFontSize;

        /// <inheritdoc />
        public bool Bold { get; set; } = false;

        /// <inheritdoc />
        public double LineSpacing { get; set; } = DefaultLineSpacing;

        /// <inheritdoc />
        public LineStyles LineStyle { get; set; } = LineStyles.None;

        /// <inheritdoc />
        public double Justification { get; set; } = 1.0;

        /// <inheritdoc />
        public override string ToString()
        {
            string[] items = [
                $"color=\"{Color}\"",
                $"opacity=\"{Opacity.ToSVG()}\"",
                $"bg=\"{Background}\"",
                $"bgo=\"{BackgroundOpacity.ToSVG()}\"",
                $"thickness=\"{LineThickness.ToSVG()}\"",
                $"fontfamily=\"{FontFamily}\"",
                $"fontsize=\"{FontSize.ToSVG()}\"",
                $"bold={(Bold ? "true" : "false")}",
                $"linestyle=\"{LineStyle}\"",
                $"justification=\"{Justification.ToSVG()}\""
                ];
            return string.Join(", ", items);
        }
    }
}
