using SimpleCircuit.Drawing;
using SkiaSharp;
using System;
using System.Net;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// A text measurer based on SkiaSharp.
    /// </summary>
    public class SkiaTextMeasurer : ITextMeasurer
    {
        private SKTypeface _typeface;
        private SKFont _font;
        private string _familyName = DefaultFontFamily, _lastFamilyName;
        private bool _reload = true;

        /// <summary>
        /// The default font family.
        /// </summary>
        public const string DefaultFontFamily = "Arial";

        /// <inheritdoc />
        public string FontFamily
        {
            get => _familyName;
            set
            {
                _familyName = value?.Trim();
                if (string.IsNullOrWhiteSpace(_familyName))
                    _familyName = DefaultFontFamily;

                // Invalidate the current typeface and font
                if (!StringComparer.Ordinal.Equals(_familyName, _lastFamilyName))
                {
                    _reload = true;
                    _lastFamilyName = _familyName;
                }
            }
        }

        /// <inheritdoc />
        public SpanBounds Measure(string text, double size)
        {
            // Check whether the font needs to be loaded
            if (_reload)
            {
                _typeface = SKFontManager.Default.MatchFamily(_familyName);
                _font = new SKFont(_typeface);
                _reload = false;
            }

            // Decode the string for HTML entities
            text = WebUtility.HtmlDecode(text);

            // Measure
            _font.Size = (float)size * 4f / 3f;
            var glyphs = _typeface.GetGlyphs(text);
            float advance = _font.MeasureText(glyphs, out var bounds);
            return new SpanBounds(new Bounds(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom), advance);
        }
    }
}
