using Microsoft.JSInterop;
using SimpleCircuit.Circuits;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.SimpleTexts;
using System.Text.Json;

namespace SimpleCircuitOnline;

/// <summary>
/// A text measurer that uses the browser.
/// </summary>
/// <remarks>
/// Creates a new <see cref="TextMeasurer"/>.
/// </remarks>
/// <param name="js">The javascript runtime.</param>
public class TextMeasurer(IJSRuntime js) : ITextMeasurer
{
    private readonly IJSRuntime _js = js;

    /// <inheritdoc />
    public SpanBounds Measure(string text, string fontFamily, bool isBold, double size)
    {
        // Replace spaces by a non-breaking space to make sure the text measuring treates leading/trailing spaces correctly
        text = text.Replace(" ", "&nbsp;");

        // Make a piece of XML that allows measuring this element
        JsonElement obj2 = ((IJSInProcessRuntime)_js).Invoke<JsonElement>("measureText", text, fontFamily, isBold, size);
        double advance = obj2.GetProperty("a").GetDouble();
        double left = obj2.GetProperty("l").GetDouble();
        double right = obj2.GetProperty("r").GetDouble();
        double top = obj2.GetProperty("t").GetDouble();
        double bottom = obj2.GetProperty("b").GetDouble();
        return new SpanBounds(new Bounds(left, top, right, bottom), advance);
    }
}
