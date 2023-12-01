using Microsoft.JSInterop;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.SimpleTexts;
using System;
using System.Text.Json;

namespace SimpleCircuitOnline
{
    /// <summary>
    /// A text measurer that uses the browser.
    /// </summary>
    public class TextMeasurer : ITextMeasurer
    {
        private readonly IJSRuntime _js;

        /// <inheritdoc />
        public string FontFamily { get; set; }

        /// <summary>
        /// Creates a new <see cref="TextMeasurer"/>.
        /// </summary>
        /// <param name="js">The javascript runtime.</param>
        public TextMeasurer(IJSRuntime js, string fontFamily)
        {
            _js = js;
            FontFamily = fontFamily;
        }

        /// <inheritdoc />
        public SpanBounds Measure(string text, double size)
        {
            // Make a piece of XML that allows measuring this element
            JsonElement obj2 = ((IJSInProcessRuntime)_js).Invoke<JsonElement>("measureText", text, FontFamily, size);
            double advance = obj2.GetProperty("a").GetDouble();
            double left = obj2.GetProperty("l").GetDouble();
            double right = obj2.GetProperty("r").GetDouble();
            double top = obj2.GetProperty("t").GetDouble();
            double bottom = obj2.GetProperty("b").GetDouble();
            return new SpanBounds(new Bounds(left, top, right, bottom), advance);
        }
    }
}
