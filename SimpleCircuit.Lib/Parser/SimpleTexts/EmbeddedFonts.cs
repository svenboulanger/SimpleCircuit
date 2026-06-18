using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleCircuit.Parser.SimpleTexts;

/// <summary>
/// Provides access to the font faces that are embedded as resources in the library.
/// These are used both for text measurement (see <see cref="FontsTextMeasurer"/>) and,
/// optionally, for embedding the font face directly into a generated SVG so that the
/// glyph shapes render identically on machines that do not have the font installed.
/// </summary>
public static class EmbeddedFonts
{
    /// <summary>
    /// The font family name of the embedded fonts.
    /// </summary>
    public const string FontFamily = "DejaVu Sans";

    private static readonly Lazy<byte[]> _regular = new(() => Load("SimpleCircuit.Fonts.DejaVuSans.ttf"));
    private static readonly Lazy<byte[]> _bold = new(() => Load("SimpleCircuit.Fonts.DejaVuSans-Bold.ttf"));

    /// <summary>
    /// Gets the raw bytes of the regular (normal weight) embedded font, or <c>null</c> if it is not available.
    /// </summary>
    public static byte[] Regular => _regular.Value;

    /// <summary>
    /// Gets the raw bytes of the bold embedded font, or <c>null</c> if it is not available.
    /// </summary>
    public static byte[] Bold => _bold.Value;

    private static byte[] Load(string resourceName)
    {
        var assembly = typeof(EmbeddedFonts).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Determines whether the given CSS font-family list refers to the embedded font family.
    /// </summary>
    /// <param name="fontFamily">The CSS font-family list (e.g. <c>"DejaVu Sans, sans-serif"</c>).</param>
    /// <returns><c>true</c> if the embedded family is referenced; otherwise, <c>false</c>.</returns>
    public static bool References(string fontFamily)
    {
        if (string.IsNullOrWhiteSpace(fontFamily))
            return false;
        foreach (string part in fontFamily.Split(','))
        {
            string name = part.Trim().Trim('\'', '"');
            if (string.Equals(name, FontFamily, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Builds a CSS stylesheet with <c>@font-face</c> rules that embed the requested embedded
    /// fonts as base64 data-URIs. This makes a generated SVG fully self-contained.
    /// </summary>
    /// <param name="includeBold">Whether to include the bold variant. The full font is large,
    /// so callers that know they do not use bold text can omit it.</param>
    /// <returns>The CSS, or an empty string when no fonts could be embedded.</returns>
    public static string CreateFontFaceStylesheet(bool includeBold = true)
    {
        var sb = new StringBuilder();
        AppendFontFace(sb, Regular, "normal", "normal");
        if (includeBold)
            AppendFontFace(sb, Bold, "normal", "bold");
        return sb.ToString();
    }

    private static void AppendFontFace(StringBuilder sb, byte[] font, string fontStyle, string fontWeight)
    {
        if (font is null || font.Length == 0)
            return;
        sb.Append("@font-face{font-family:\"").Append(FontFamily).Append("\";");
        sb.Append("font-style:").Append(fontStyle).Append(';');
        sb.Append("font-weight:").Append(fontWeight).Append(';');
        sb.Append("src:url(\"data:font/ttf;base64,");
        sb.Append(Convert.ToBase64String(font));
        sb.Append("\") format(\"truetype\");}");
    }
}
