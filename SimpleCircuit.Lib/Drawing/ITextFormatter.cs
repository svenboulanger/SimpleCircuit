namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Represents a text formatter.
    /// </summary>
    public interface ITextFormatter
    {
        /// <summary>
        /// Formats a line of text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="size">The size of the text.</param>
        /// <returns>The formatted text.</returns>
        public FormattedText Format(string text, double size);
    }
}
