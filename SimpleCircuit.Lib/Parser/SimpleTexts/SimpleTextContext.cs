using SimpleCircuit.Drawing.Styles;
using System.Text;

namespace SimpleCircuit.Parser.SimpleTexts;

/// <summary>
/// A context for parsing SimpleCircuit text.
/// </summary>
/// <remarks>
/// Creates a new <see cref="SimpleTextContext"/>.
/// </remarks>
public class SimpleTextContext(ITextMeasurer measurer)
{
    /// <summary>
    /// Gets the text measurer.
    /// </summary>
    public ITextMeasurer Measurer { get; } = measurer ?? new SkiaTextMeasurer();

    /// <summary>
    /// Gets the text builder.
    /// </summary>
    public StringBuilder Builder { get; } = new();

    /// <summary>
    /// Gets or sets the current appearance.
    /// </summary>
    public IStyle Style { get; set; }
}
