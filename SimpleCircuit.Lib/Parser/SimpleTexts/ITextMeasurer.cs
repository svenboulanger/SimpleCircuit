namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// An interface that describes a text measurer.
    /// </summary>
    public interface ITextMeasurer
    {
        /// <summary>
        /// Gets the font family name.
        /// </summary>
        public string FontFamily { get; set; }

        /// <summary>
        /// Measures a string of a given size. The bounds should be given with
        /// the origin being the start of the baseline of the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="isBold">If <c>true</c>, the text is bold text.</param>
        /// <param name="size">The size.</param>
        /// <returns>The bounds.</returns>
        public SpanBounds Measure(string text, bool isBold, double size);
    }
}
