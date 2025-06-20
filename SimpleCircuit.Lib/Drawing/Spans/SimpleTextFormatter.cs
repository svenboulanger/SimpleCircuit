﻿using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Parser.SimpleTexts;

namespace SimpleCircuit.Drawing.Spans
{
    /// <summary>
    /// A simple text formatter.
    /// </summary>
    /// <param name="measurer">The text measurer.</param>
    public class SimpleTextFormatter(ITextMeasurer measurer) : ITextFormatter
    {
        /// <summary>
        /// Gets the text measurer.
        /// </summary>
        public ITextMeasurer Measurer { get; } = measurer;

        /// <inheritdoc />
        public Span Format(string content, IStyle appearance)
        {
            content = content.Replace("<", "&lt;").Replace(">", "&gt;");

            var lexer = new SimpleTextLexer(content);
            var context = new SimpleTextContext(Measurer)
            {
                Style = appearance
            };
            return SimpleTextParser.Parse(lexer, context);
        }
    }
}
