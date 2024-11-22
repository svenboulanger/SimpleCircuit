using SimpleCircuit.Components.Builders;
using SimpleCircuit.Parser.SimpleTexts;

namespace SimpleCircuit.Circuits.Spans
{
    /// <summary>
    /// A simple text formatter.
    /// </summary>
    /// <param name="measurer">The text measurer.</param>
    public class SimpleTextFormatter(ITextMeasurer measurer) : ITextFormatter
    {
        /// <inheritdoc />
        public string FontFamily
        {
            get => Measurer.FontFamily;
            set => Measurer.FontFamily = value;
        }

        /// <summary>
        /// Gets the text measurer.
        /// </summary>
        public ITextMeasurer Measurer { get; } = measurer;

        /// <inheritdoc />
        public Span Format(string content, double fontSize = 4, bool isBold = false, GraphicOptions options = null)
        {
            content = content.Replace("<", "&lt;").Replace(">", "&gt;");

            var lexer = new SimpleTextLexer(content);
            var context = new SimpleTextContext(Measurer)
            {
                FontSize = fontSize,
                IsBold = isBold
            };
            return SimpleTextParser.Parse(lexer, context);
        }
    }
}
