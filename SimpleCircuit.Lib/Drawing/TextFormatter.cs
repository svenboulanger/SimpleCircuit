namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// A simple text formatter.
    /// </summary>
    public class TextFormatter : ITextFormatter
    {
        /// <summary>
        /// Represents a line height relative to the font size.
        /// </summary>
        public double LineHeight { get; set; } = 5.0 / 3.0;

        /// <summary>
        /// Represents a width for a lowercase character relative to the font size.
        /// </summary>
        public double LowerCharacterWidth { get; set; } = 1.0;

        /// <summary>
        /// Represents a width for an uppercase character relative to the font size..
        /// </summary>
        public double UpperCharacterWidth { get; set; } = 1.25;

        /// <summary>
        /// Gets the middle-line factor.
        /// </summary>
        public double MidLineFactor { get; set; } = 0.125;

        /// <inheritdoc />
        public FormattedText Format(string text, double size)
        {
            double w = 0;
            foreach (var c in text)
                w += char.IsLower(c) ? LowerCharacterWidth : UpperCharacterWidth;
            return new(text, new(0, -size * (1 - MidLineFactor), w * size, size * MidLineFactor));
        }
    }
}
