using SimpleCircuit.Components.Styles;

namespace SimpleCircuit.Circuits.Spans
{
    /// <summary>
    /// Describes a text formatter.
    /// </summary>
    public interface ITextFormatter
    {
        /// <summary>
        /// Formats text into a span.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="parentAppearance">The parent appearance options.</param>
        /// <returns>Returns the formatted content.</returns>
        public Span Format(string content, IStyle parentAppearance);
    }
}
