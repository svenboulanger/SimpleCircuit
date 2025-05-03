using SimpleCircuit.Components;
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
        public Span Format(string content, AppearanceOptions appearance)
        {
            content = content.Replace("<", "&lt;").Replace(">", "&gt;");

            var lexer = new SimpleTextLexer(content);
            var context = new SimpleTextContext(Measurer)
            {
                FontSize = appearance.FontSize,
                IsBold = appearance.Bold,
                Color = appearance.Color,
                Opacity = appearance.Opacity,
            };
            return SimpleTextParser.Parse(lexer, context);
        }
    }
}
