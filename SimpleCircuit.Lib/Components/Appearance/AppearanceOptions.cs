namespace SimpleCircuit.Components.Appearance
{
    /// <summary>
    /// Describes appearance options that are derived from another.
    /// </summary>
    public class AppearanceOptions : IAppearanceOptions
    {
        private string _color, _background, _fontFamily;
        private double? _opacity, _backgroundOpacity, _lineThickness, _fontSize, _lineSpacing;
        private bool? _bold;
        private int? _lineStyle;

        public const string Black = "black";
        public const string None = "none";
        public const double Opaque = 1.0;
        public const double DefaultLineThickness = 0.5;
        public const string DefaultFontFamily = "Arial";
        public const double DefaultFontSize = 4.0;
        public const double DefaultLineSpacing = 1.5;
        public const int DefaultLineStyle = -1;

        /// <inheritdoc />
        public IAppearanceOptions Parent { get; set; }

        /// <inheritdoc />
        public string Color { get => _color ?? Parent?.Color ?? Black; set => _color = value; }

        /// <inheritdoc />
        public double Opacity { get => _opacity ?? Parent?.Opacity ?? Opaque; set => _opacity = value; }

        /// <inheritdoc />
        public string Background { get => _background ?? Parent?.Background ?? None; set => _background = value; }

        /// <inheritdoc />
        public double BackgroundOpacity { get => _backgroundOpacity ?? Parent?.BackgroundOpacity ?? Opaque; set => _backgroundOpacity = value; }

        /// <inheritdoc />
        public double LineThickness { get => _lineThickness ?? Parent?.LineThickness ?? DefaultLineThickness; set => _lineThickness = value; }

        /// <inheritdoc />
        public string FontFamily { get => _fontFamily ?? Parent?.FontFamily ?? DefaultFontFamily; set => _fontFamily = value; }

        /// <inheritdoc />
        public double FontSize { get => _fontSize ?? Parent?.FontSize ?? DefaultFontSize; set => _fontSize = value; }

        /// <inheritdoc />
        public bool Bold { get => _bold ?? Parent?.Bold ?? false; set => _bold = value; }

        /// <inheritdoc />
        public double LineSpacing { get => _lineSpacing ?? Parent?.LineSpacing ?? DefaultLineSpacing; set => _lineSpacing = value; }

        /// <inheritdoc />
        public int LineStyle { get => _lineStyle ?? Parent?.LineStyle ?? DefaultLineStyle; set => _lineStyle = value; }
    }
}
