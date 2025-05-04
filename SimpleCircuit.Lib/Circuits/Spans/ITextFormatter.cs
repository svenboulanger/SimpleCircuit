using SimpleCircuit.Components.Appearance;
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
        /// <param name="parentAppearance">The parent appearance options.</param>
        /// <returns>Returns the formatted content.</returns>
        public Span Format(string content, IAppearanceOptions parentAppearance);
    }
}
