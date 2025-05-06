using SimpleCircuit.Circuits;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// An interface that describes a text measurer.
    /// </summary>
    public interface ITextMeasurer
    {
        /// <summary>
        /// Measures a string of a given size. The bounds should be given with
        /// the origin being the start of the baseline of the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="isBold">If <c>true</c>, the text is bold text.</param>
        /// <param name="size">The size.</param>
        /// <returns>The bounds.</returns>
        public SpanBounds Measure(string text, string fontFamily, bool isBold, double size);
    }
}
