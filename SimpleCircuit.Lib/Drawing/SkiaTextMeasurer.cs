using SkiaSharp;
using System;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// A text measurer based on SkiaSharp.
    /// </summary>
    public class SkiaTextMeasurer : ITextMeasurer
    {
        private readonly SKTypeface _typeface;
        private readonly SKFont _font;

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
            _font.Size = (float)size * 1.333f;
            var glyphs = _typeface.GetGlyphs(text);
            _font.MeasureText(glyphs, out var bounds);
            return new Bounds(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        }
    }
}
