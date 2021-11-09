namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// A formatted piece of text.
    /// The baseline is assumes to be at a height of 0.
    /// </summary>
    public struct FormattedText
    {
        /// <summary>
        /// Gets the contents of the text.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets the bounds of the text. The anchor is
        /// assumed left of the text at the baseline.
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// Creates new formatted text.
        /// </summary>
        /// <param name="content">The formatted contents.</param>
        /// <param name="bounds">The bounds.</param>
        public FormattedText(string content, Bounds bounds)
        {
            Content = content;
            Bounds = bounds;
        }

        /// <summary>
        /// Converts the formatted text to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => Content;
    }
}
