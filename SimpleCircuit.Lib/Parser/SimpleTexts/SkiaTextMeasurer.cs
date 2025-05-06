using SimpleCircuit.Circuits;
using SimpleCircuit.Drawing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// A text measurer based on SkiaSharp.
    /// </summary>
    public class SkiaTextMeasurer : ITextMeasurer
    {
        private struct TypefaceKey : IEquatable<TypefaceKey>
        {
            public string FontFamily { get; }
            public bool IsBold { get; }
            public TypefaceKey(string fontFamily, bool isBold)
            {
                FontFamily = fontFamily;
                IsBold = isBold;
            }
            public override int GetHashCode() => (FontFamily.GetHashCode() * 1021) ^ IsBold.GetHashCode();
            public override bool Equals(object? other) => other is TypefaceKey tk && Equals(tk);
            public bool Equals(TypefaceKey other) => other.FontFamily.Equals(FontFamily) && IsBold == other.IsBold;
        }

        private readonly Dictionary<TypefaceKey, (SKTypeface, SKFont)> _fonts = [];

        /// <summary>
        /// The default font family.
        /// </summary>
        public const string DefaultFontFamily = "Arial";

        /// <inheritdoc />
        public SpanBounds Measure(string text, string fontFamily, bool isBold, double size)
        {
            // Check whether the font needs to be loaded
            var key = new TypefaceKey(fontFamily, isBold);
            if (!_fonts.TryGetValue(key, out var font))
            {
                var typeface = SKFontManager.Default.MatchFamily(fontFamily, isBold ? SKFontStyle.Bold : SKFontStyle.Normal);
                font = (typeface, new SKFont(typeface));
                _fonts[key] = font;
            }

            // Decode the string for HTML entities
            text = WebUtility.HtmlDecode(text);

            // Measure
            font.Item2.Size = (float)size * 4f / 3f;
            var glyphs = font.Item1.GetGlyphs(text);
            float advance = font.Item2.MeasureText(glyphs, out var bounds);
            return new SpanBounds(new Bounds(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom), advance);
        }
    }
}
