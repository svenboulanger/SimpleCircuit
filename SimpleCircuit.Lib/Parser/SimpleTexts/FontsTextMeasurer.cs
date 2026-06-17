using SimpleCircuit.Circuits;
using SimpleCircuit.Drawing;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SimpleCircuit.Parser.SimpleTexts;

/// <summary>
/// A text measurer based on SixLabors.Fonts. Fonts are resolved from an embedded
/// font shipped with the library first, falling back to the system fonts, so that
/// text measurement does not depend on any particular font being installed.
/// </summary>
public class FontsTextMeasurer : ITextMeasurer
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
        public override bool Equals(object other) => other is TypefaceKey tk && Equals(tk);
        public bool Equals(TypefaceKey other) => other.FontFamily.Equals(FontFamily) && IsBold == other.IsBold;
    }

    /// <summary>
    /// The default font family.
    /// </summary>
    public const string DefaultFontFamily = "DejaVu Sans";

    // Fonts embedded as resources in the library. These guarantee consistent text
    // measurement regardless of the fonts installed on the host machine.
    private static readonly FontCollection _embedded;
    private static readonly FontFamily _fallback;

    private readonly Dictionary<TypefaceKey, Font> _fonts = [];

    static FontsTextMeasurer()
    {
        _embedded = new FontCollection();
        var assembly = typeof(FontsTextMeasurer).Assembly;
        FontFamily fallback = default;
        foreach (string name in new[] { "SimpleCircuit.Fonts.DejaVuSans.ttf", "SimpleCircuit.Fonts.DejaVuSans-Bold.ttf" })
        {
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream is null)
                continue;

            // Copy to a seekable in-memory stream; the font collection may read it lazily.
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            fallback = _embedded.Add(ms);
        }
        _fallback = fallback;
    }

    /// <summary>
    /// Resolves a CSS-like font family list to a concrete font family. Each comma
    /// separated family is tried in order against the embedded fonts and then the
    /// system fonts; the embedded default is used when nothing matches.
    /// </summary>
    private static FontFamily ResolveFamily(string fontFamily)
    {
        if (!string.IsNullOrWhiteSpace(fontFamily))
        {
            foreach (string part in fontFamily.Split(','))
            {
                string name = part.Trim().Trim('\'', '"');
                if (name.Length == 0)
                    continue;
                if (_embedded.TryGet(name, out var family) || SystemFonts.TryGet(name, out family))
                    return family;
            }
        }
        return _fallback;
    }

    /// <inheritdoc />
    public SpanBounds Measure(string text, string fontFamily, bool isBold, double size)
    {
        // Check whether the font needs to be loaded
        var key = new TypefaceKey(fontFamily, isBold);
        if (!_fonts.TryGetValue(key, out var font))
        {
            var family = ResolveFamily(fontFamily);
            font = family.CreateFont((float)size, isBold ? FontStyle.Bold : FontStyle.Regular);
            _fonts[key] = font;
        }

        // Decode the string for HTML entities
        text = WebUtility.HtmlDecode(text);

        // Measure at 96 DPI so the point size matches the previous 4/3 (px-per-pt) scaling.
        var options = new TextOptions(font) { Dpi = 96f };
        var bounds = TextMeasurer.MeasureBounds(text, options);
        float advance = TextMeasurer.MeasureAdvance(text, options).Width;

        // SixLabors.Fonts lays text out with the origin at the top of the line and y
        // increasing downwards. The measurer contract expects bounds relative to the
        // start of the baseline, so shift up by the (scaled) ascender.
        var metrics = font.FontMetrics;
        double baseline = metrics.HorizontalMetrics.Ascender / (double)metrics.UnitsPerEm * size * 96.0 / 72.0;
        return new SpanBounds(new Bounds(bounds.Left, bounds.Top - baseline, bounds.Right, bounds.Bottom - baseline), advance);
    }
}
