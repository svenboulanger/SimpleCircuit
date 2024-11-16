using System.Collections.Generic;

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
        public static ISpan Parse(SimpleTextLexer lexer, SimpleTextContext context)
        {
            var lines = new List<ISpan>();

            // Parse lines
            while (lexer.Type != TokenType.EndOfContent)
            {
                lines.Add(ParseLine(lexer, context));
                lexer.Branch(TokenType.Newline);
            }
            return new MultilineSpan(lines, context.FontSize * context.LineSpacing, context.Align);
        }
        private static ISpan ParseLine(SimpleTextLexer lexer, SimpleTextContext context)
        {
            // Parses a complete line
            var result = new LineSpan();
            while (lexer.Check(~TokenType.Newline))
            {
                result.Add(ParseSuperSubScript(lexer, context));
            }
            return result;
        }
        private static ISpan ParseSuperSubScript(SimpleTextLexer lexer, SimpleTextContext context)
        {
            var result = ParseSegment(lexer, context);

            // Deal with super/subscripts
            if (lexer.Check(TokenType.Superscript | TokenType.Subscript))
            {
                // Create our sub/superscript element and make the font size smaller for whatever is next
                double oldFontSize = context.FontSize;
                ISpan sub = null, super = null;
                context.FontSize *= 0.8;
                if (lexer.Branch(TokenType.Subscript))
                {
                    if (lexer.Branch(TokenType.OpenBracket))
                    {
                        sub = ParseBlockSegment(lexer, context);
                        lexer.Branch(TokenType.CloseBracket);
                    }
                    else
                        sub = ParseSegment(lexer, context);
                    if (lexer.Branch(TokenType.Superscript))
                    {
                        if (lexer.Branch(TokenType.OpenBracket))
                        {
                            super = ParseBlockSegment(lexer, context);
                            lexer.Branch(TokenType.CloseBracket);
                        }
                        else
                            super = ParseSegment(lexer, context);
                    }
                }
                else if (lexer.Branch(TokenType.Superscript))
                {
                    if (lexer.Branch(TokenType.OpenBracket))
                    {
                        super = ParseBlockSegment(lexer, context);
                        lexer.Branch(TokenType.CloseBracket);
                    }
                    else
                        super = ParseLine(lexer, context);
                    if (lexer.Branch(TokenType.Subscript))
                    {
                        if (lexer.Branch(TokenType.OpenBracket))
                        {
                            sub = ParseBlockSegment(lexer, context);
                            lexer.Branch(TokenType.CloseBracket);
                        }
                        else
                            sub = ParseBlockSegment(lexer, context);
                    }
                }
                context.FontSize = oldFontSize;
                return new SubscriptSuperscriptSpan(result, sub, super, 0.5 * context.FontSize, new(0, 0.05 * context.FontSize));
            }
            return result;
        }
        private static ISpan ParseBlockSegment(SimpleTextLexer lexer, SimpleTextContext context)
        {
            // Parses everything until the next block segment
            var result = new LineSpan();
            while (lexer.Check(~TokenType.Newline & ~TokenType.CloseBracket))
            {
                result.Add(ParseSuperSubScript(lexer, context));
            }
            return result;
        }
        private static ISpan ParseSegment(SimpleTextLexer lexer, SimpleTextContext context)
        {
            // Parses a segment that cannot be separated
            switch (lexer.Type)
            {
                case TokenType.EscapedSequence:
                    // Check for an escape sequence
                    string content = lexer.Content.ToString();
                    switch (content)
                    {
                        case "\\overline":
                            lexer.Next();
                            if (lexer.Branch(TokenType.OpenBracket))
                            {
                                var b = ParseBlockSegment(lexer, context);
                                lexer.Branch(TokenType.CloseBracket);
                                return CreateOverline(b, context);
                            }
                            else
                            {
                                context.Builder.Append(content);
                                ContinueText(lexer, context);
                                return CreateTextSpan(context);
                            }

                        case "\\underline":
                            lexer.Next();
                            if (lexer.Branch(TokenType.OpenBracket))
                            {
                                var b = ParseBlockSegment(lexer, context);
                                lexer.Branch(TokenType.CloseBracket);
                                return CreateUnderline(b, context);
                            }
                            else
                            {
                                context.Builder.Append(content);
                                ContinueText(lexer, context);
                                return CreateTextSpan(context);
                            }

                        default:
                            context.Builder.Append(content);
                            lexer.Next();
                            ContinueText(lexer, context);
                            return CreateTextSpan(context);
                    }

                case TokenType.OpenBracket:
                case TokenType.CloseBracket:
                    // This might happen if there are too many curly brackets somewhere...
                    context.Builder.Append(lexer.Content.ToString());
                    lexer.Next();
                    ContinueText(lexer, context);
                    return CreateTextSpan(context);

                default:
                    ContinueText(lexer, context);
                    return CreateTextSpan(context);
            }
        }
        private static void ContinueText(SimpleTextLexer lexer, SimpleTextContext context)
        {
            while (lexer.Check(TokenType.Character | TokenType.EscapedCharacter))
            {
                if (lexer.Type == TokenType.EscapedCharacter)
                    context.Builder.Append(lexer.Content.Span[1]);
                else
                    context.Builder.Append(lexer.Content.ToString());
                lexer.Next();
            }
        }
        private static ISpan CreateOverline(ISpan @base, SimpleTextContext context)
        {
            double margin = context.FontSize * 0.1;
            double thickness = context.FontSize * 0.075;

            return new OverlineSpan(@base, margin, thickness);
        }
        private static ISpan CreateUnderline(ISpan @base, SimpleTextContext context)
        {
            double margin = context.FontSize * 0.1;
            double thickness = context.FontSize * 0.075;

            return new UnderlineSpan(@base, margin, thickness);
        }
        private static ISpan CreateTextSpan(SimpleTextContext context)
        {
            // Measure the contents
            string content = context.Builder.ToString();
            context.Builder.Clear();
            var bounds = context.Measurer.Measure(content, context.IsBold, context.FontSize);

            // Return the span
            return new TextSpan(content, context.Measurer.FontFamily, false, context.FontSize, bounds);
        }
    }
}
