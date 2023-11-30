using Microsoft.JSInterop;
using SimpleCircuit.Drawing;
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
        public Bounds Measure(string text, double size)
        {
            // Make a piece of XML that allows measuring this element
            string xml = $"<svg class=\"simplecircuit\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 1 1\" width=\"512px\" height=\"512px\"><text><tspan x=\"0\" y=\"0\" style=\"font-family: {FontFamily}; font-size: {size}pt; stroke: none; fill: black;\">{text}</tspan></text></svg>";

            JsonElement obj = ((IJSInProcessRuntime)_js).Invoke<JsonElement>("calculateBounds", xml);
            double x = obj.GetProperty("x").GetDouble();
            double y = obj.GetProperty("y").GetDouble();
            double width = obj.GetProperty("width").GetDouble();
            double height = obj.GetProperty("height").GetDouble();
            return new Bounds(x, y, x + width, y + height);
        }
    }
}
