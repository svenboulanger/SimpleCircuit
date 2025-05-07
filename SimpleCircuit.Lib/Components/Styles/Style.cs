using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// Describes appearance options that are derived from another.
    /// </summary>
    public class Style : IStyle
    {
        private string _color, _background, _fontFamily;
        private double? _opacity, _backgroundOpacity, _lineThickness, _fontSize, _lineSpacing;
        private bool? _bold;
        private LineStyles? _lineStyle;

        public const string Black = "black";
        public const string None = "none";
        public const double Opaque = 1.0;
        public const double DefaultLineThickness = 0.5;
        public const string DefaultFontFamily = "Arial";
        public const double DefaultFontSize = 4.0;
        public const double DefaultLineSpacing = 1.5;
        public const LineStyles DefaultLineStyle = LineStyles.None;

        /// <inheritdoc />
        public IStyle Parent { get; set; }

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
        public LineStyles LineStyle { get => _lineStyle ?? Parent?.LineStyle ?? DefaultLineStyle; set => _lineStyle = value; }

        public override string ToString()
        {
            var items = new List<string>();
            if (_color is not null)
                items.Add($"color=\"{_color}\"");
            if (_opacity is not null)
                items.Add($"opacity=\"{_opacity.Value.ToString(CultureInfo.InvariantCulture)}\"");
            if (_background is not null)
                items.Add($"bg=\"{_background}\"");
            if (_backgroundOpacity is not null)
                items.Add($"bgo=\"{_backgroundOpacity.Value.ToString(CultureInfo.InvariantCulture)}\"");
            if (_lineThickness is not null)
                items.Add($"thickness={_lineThickness.Value.ToString(CultureInfo.InvariantCulture)}");
            if (_fontFamily is not null)
                items.Add($"fontfamily=\"{_fontFamily}\"");
            if (_fontSize is not null)
                items.Add($"fontsize=\"{_fontSize}\"");
            if (_bold is not null)
                items.Add($"bold={_bold}");
            if (_lineSpacing is not null)
                items.Add($"linespacing={_lineSpacing.Value.ToString(CultureInfo.InvariantCulture)}");
            if (_lineStyle is not null)
                items.Add(_lineStyle.ToString().ToLower());
            return string.Join(", ", items);
        }
    }
}
