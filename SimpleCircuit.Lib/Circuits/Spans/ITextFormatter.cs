using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Circuits.Spans
{
    /// <summary>
    /// Describes a text formatter.
    /// </summary>
    public interface ITextFormatter
    {
        /// <summary>
        /// Gets or sets the font family of the formatter.
        /// </summary>
        public string FontFamily { get; set; }

        /// <summary>
        /// Formats text into a span.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="fontSize">The font size.</param>
        /// <param name="isBold">If <c>true</c>, the content is supposed to be bold.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>Returns the formatted content.</returns>
        public Span Format(string content, double fontSize = 4.0, bool isBold = false, GraphicOptions options = null);
    }
}
