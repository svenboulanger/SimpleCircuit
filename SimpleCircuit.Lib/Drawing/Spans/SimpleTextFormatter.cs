using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Parser.SimpleTexts;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleCircuit.Drawing.Spans;

/// <summary>
/// A simple text formatter.
/// </summary>
/// <param name="measurer">The text measurer.</param>
public class SimpleTextFormatter(ITextMeasurer measurer) : ITextFormatter
{
    /// <summary>
    /// Gets the text measurer.
    /// </summary>
    public ITextMeasurer Measurer { get; } = measurer;

    // Matches an HTML reference (numeric or named) or a lone ampersand. Numeric refs
    // are passed through unchanged, named refs are translated to numeric refs and a
    // lone ampersand is escaped.
    private static readonly Regex _entityRegex = new(@"&(#x[0-9a-fA-F]+|#[0-9]+|[A-Za-z][A-Za-z0-9]*);|&", RegexOptions.Compiled);

    /// <inheritdoc />
    public Span Format(string content, IStyle appearance)
    {
        if (string.IsNullOrEmpty(content))
            return new TextSpan(string.Empty, appearance, new Circuits.SpanBounds(new(0, 0, 0, 0), 0));

        // Translate HTML entities to numeric character references. SVG/XML only knows
        // the five predefined entities, so named entities such as &Omega; would break
        // InnerXml. Numeric refs are valid XML and decode to the same glyph during
        // measurement, keeping layout and rendering in sync. This must run before the
        // '<'/'>' escaping below so the '&' it introduces is not double-escaped.
        content = _entityRegex.Replace(content, TranslateEntity);
        content = content.Replace("<", "&lt;").Replace(">", "&gt;");

        var lexer = new SimpleTextLexer(content);
        var context = new SimpleTextContext(Measurer)
        {
            Style = appearance
        };
        return SimpleTextParser.Parse(lexer, context);
    }

    /// <summary>
    /// Translates a single regex match (an HTML reference or a lone ampersand) into
    /// valid XML.
    /// </summary>
    private static string TranslateEntity(Match match)
    {
        var reference = match.Groups[1];
        if (!reference.Success)
            return "&amp;"; // A lone ampersand that is not part of a reference.

        string body = reference.Value;
        if (body[0] == '#')
            return match.Value; // Numeric character reference, already valid XML.

        // Curated fast-path for the blessed keywords.
        if (TextEntities.Entities.TryGetValue(body, out int codePoint))
            return $"&#{codePoint};";

        // Fall back to the full set of HTML entities the framework knows about.
        string decoded = WebUtility.HtmlDecode(match.Value);
        if (decoded != match.Value)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < decoded.Length;)
            {
                int codepoint = char.ConvertToUtf32(decoded, i);
                sb.Append("&#").Append(codepoint).Append(';');
                i += char.IsSurrogatePair(decoded, i) ? 2 : 1;
            }
            return sb.ToString();
        }

        // Truly unknown name: escape the ampersand so it renders literally as text.
        return $"&amp;{body};";
    }
}
