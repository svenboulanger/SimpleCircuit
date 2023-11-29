using SkiaSharp;
using System;
using System.Text.RegularExpressions;
using System.Net;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// A text measurer based on SkiaSharp.
    /// </summary>
    public class SkiaTextMeasurer : ITextMeasurer
    {
        private readonly SKTypeface _typeface;
        private readonly SKFont _font;
        private static readonly Regex _unicode = new Regex(@"&#(?<value>\d+|x[\da-fA-F]+);", RegexOptions.Compiled);
        private static readonly Regex _entities = new Regex(@"&(?<value>\w+);", RegexOptions.Compiled);

        /// <inheritdoc />
        public string FontFamily => _typeface.FamilyName;

        /// <summary>
        /// Creates a new text measurer.
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        public SkiaTextMeasurer(string fontFamily)
        {
            _typeface = SKFontManager.Default.MatchFamily(fontFamily);
            _font = new SKFont(_typeface);
        }

        /// <inheritdoc />
        public Bounds Measure(string text, double size)
        {
            // Decode the string for HTML entities
            text = WebUtility.HtmlDecode(text);

            // Measure
            _font.Size = (float)size * 1.333f;
            var glyphs = _typeface.GetGlyphs(text);
            _font.MeasureText(glyphs, out var bounds);
            return new Bounds(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        }
    }
}
