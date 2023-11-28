namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// Parser methods for SimpleCircuit text.
    /// </summary>
    public static class SimpleTextParser
    {
        /// <summary>
        /// Parses some text for layouting.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The context.</param>
        public static void Parse(SimpleTextLexer lexer, SimpleTextContext context)
        {
            while (lexer.Type != TokenType.EndOfContent)
                ParseToken(lexer, context);
        }

        private static void ParseToken(SimpleTextLexer lexer, SimpleTextContext context)
        {
            switch (lexer.Type)
            {
                case TokenType.Subscript:
                    lexer.Next();
                    ParseSubscript(lexer, context);
                    break;

                case TokenType.Superscript:
                    lexer.Next();
                    ParseSuperscript(lexer, context);
                    break;

                case TokenType.Escaped:
                    context.Append(lexer.Content.Span[1]);
                    lexer.Next();
                    break;

                case TokenType.Newline:
                    context.FinishLine();
                    lexer.Next();
                    break;

                default:
                    context.Append(lexer.Content.ToString());
                    lexer.Next();
                    break;
            }
        }

        private static void ParseSubscript(SimpleTextLexer lexer, SimpleTextContext context)
        {
            double shift = context.RelativeFontWeight * 0.375;
            context.BaseLineOffset += shift;
            context.RelativeFontWeight *= 0.75;
            if (lexer.Branch(TokenType.OpenBracket))
            {
                // Parse until a closing bracket
                while (lexer.Check(~TokenType.Newline & ~TokenType.CloseBracket))
                    ParseToken(lexer, context);
                lexer.Branch(TokenType.CloseBracket);
            }
            else
            {
                // Parse until a whitespace
                while (lexer.Check(~TokenType.Newline & ~TokenType.CloseBracket & ~TokenType.Subscript & ~TokenType.Superscript) && lexer.Content.Span[0] != ' ')
                    ParseToken(lexer, context);
            }
            context.BaseLineOffset -= shift;
            context.RelativeFontWeight /= 0.75;
        }

        private static void ParseSuperscript(SimpleTextLexer lexer, SimpleTextContext context)
        {
            double shift = context.RelativeFontWeight * 0.375;
            context.BaseLineOffset -= shift;
            context.RelativeFontWeight *= 0.75;
            if (lexer.Branch(TokenType.OpenBracket))
            {
                // Parse until a closing bracket
                while (lexer.Check(~TokenType.Newline & ~TokenType.CloseBracket & ~TokenType.Superscript & ~TokenType.Subscript))
                    ParseToken(lexer, context);
                lexer.Branch(TokenType.CloseBracket);
            }
            else
            {
                // Parse until a whitespace
                while (lexer.Check(~TokenType.Newline & ~TokenType.CloseBracket & ~TokenType.Subscript & ~TokenType.Superscript) && lexer.Content.Span[0] != ' ')
                    ParseToken(lexer, context);
            }
            context.BaseLineOffset += shift;
            context.RelativeFontWeight /= 0.75;
        }
    }
}
