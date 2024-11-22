using Microsoft.JSInterop;
using SimpleCircuit.Circuits;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.SimpleTexts;
using System.Text.Json;

namespace SimpleCircuitOnline
{
    /// <summary>
    /// A text measurer that uses the browser.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="TextMeasurer"/>.
    /// </remarks>
    /// <param name="js">The javascript runtime.</param>
    public class TextMeasurer(IJSRuntime js, string fontFamily) : ITextMeasurer
    {
        private readonly IJSRuntime _js = js;

        /// <inheritdoc />
        public string FontFamily { get; set; } = fontFamily;

        /// <inheritdoc />
        public SpanBounds Measure(string text, bool isBold, double size)
        {
            // Make a piece of XML that allows measuring this element
            JsonElement obj2 = ((IJSInProcessRuntime)_js).Invoke<JsonElement>("measureText", text, FontFamily, isBold, size);
            double advance = obj2.GetProperty("a").GetDouble();
            double left = obj2.GetProperty("l").GetDouble();
            double right = obj2.GetProperty("r").GetDouble();
            double top = obj2.GetProperty("t").GetDouble();
            double bottom = obj2.GetProperty("b").GetDouble();
            return new SpanBounds(new Bounds(left, top, right, bottom), advance);
        }
    }
}
