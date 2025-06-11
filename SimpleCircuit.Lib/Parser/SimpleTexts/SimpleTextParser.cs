using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;

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
        public static Span Parse(SimpleTextLexer lexer, SimpleTextContext context)
        {
            var lines = new MultilineSpan(context.Style.FontSize * context.Style.LineSpacing, context.Style.Justification);

            // Parse lines
            while (lexer.Type != TokenType.EndOfContent)
            {
                lines.Add(ParseLine(lexer, context));
                lexer.Branch(TokenType.Newline);
            }

            lines.SetOffset(default);
            return lines;
        }
        private static Span ParseLine(SimpleTextLexer lexer, SimpleTextContext context)
        {
            // Parses a complete line
            var result = new LineSpan();
            while (lexer.Check(~TokenType.Newline))
            {
                result.Add(ParseSuperSubScript(lexer, context));
            }
            return result;
        }
        private static Span ParseSuperSubScript(SimpleTextLexer lexer, SimpleTextContext context)
        {
            var result = ParseSegment(lexer, context);

            // Deal with super/subscripts
            if (lexer.Check(TokenType.Superscript | TokenType.Subscript))
            {
                // Create our sub/superscript element and make the font size smaller for whatever is next
                var oldStyle = context.Style;
                context.Style = new FontSizeStyleModifier.Style(context.Style, context.Style.FontSize * 0.8);
                Span sub = null, super = null;
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
                context.Style = oldStyle;
                return new SubscriptSuperscriptSpan(result, sub, super, 0.5 * context.Style.FontSize, new(0, 0.075 * context.Style.FontSize));
            }
            return result;
        }
        private static Span ParseBlockSegment(SimpleTextLexer lexer, SimpleTextContext context)
        {
            // Parses everything until the next block segment
            var result = new LineSpan();
            while (lexer.Check(~TokenType.Newline & ~TokenType.CloseBracket))
            {
                result.Add(ParseSuperSubScript(lexer, context));
            }
            return result;
        }
        private static Span ParseSegment(SimpleTextLexer lexer, SimpleTextContext context)
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

                        case "\\textcolor":
                            var track = lexer.Track();
                            lexer.Next();
                            if (lexer.Branch(TokenType.OpenBracket))
                            {
                                // Read the color first
                                var oldStyle = context.Style;
                                context.Style = ParseColorStyle(lexer, context);

                                if (lexer.Branch(TokenType.OpenBracket))
                                {
                                    var b = ParseBlockSegment(lexer, context);
                                    lexer.Branch(TokenType.CloseBracket);
                                    context.Style = oldStyle;
                                    return b;
                                }
                                else
                                {
                                    context.Style = oldStyle;
                                    context.Builder.Append(lexer.GetTracked(track).Content);
                                    ContinueText(lexer, context);
                                    return CreateTextSpan(context);
                                }
                            }
                            else
                            {
                                context.Builder.Append(content);
                                lexer.Next();
                                ContinueText(lexer, context);
                                return CreateTextSpan(context);
                            }

                        case "\\textb":
                            lexer.Next();
                            if (lexer.Branch(TokenType.OpenBracket))
                            {
                                var oldStyle = context.Style;
                                context.Style = new BoldTextStyleModifier.Style(oldStyle);
                                var b = ParseBlockSegment(lexer, context);
                                lexer.Branch(TokenType.CloseBracket);
                                context.Style = oldStyle;
                                return b;
                            }
                            else
                            {
                                context.Builder.Append(context);
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
        private static Span CreateOverline(Span @base, SimpleTextContext context)
        {
            double margin = context.Style.FontSize * 0.1;
            var style = new StrokeWidthStyleModifier.Style(context.Style, context.Style.FontSize * 0.075);
            return new OverlineSpan(@base, margin, style);
        }
        private static Span CreateUnderline(Span @base, SimpleTextContext context)
        {
            double margin = context.Style.FontSize * 0.1;
            var style = new StrokeWidthStyleModifier.Style(context.Style, context.Style.FontSize * 0.075);
            return new UnderlineSpan(@base, margin, style);
        }
        private static Span CreateTextSpan(SimpleTextContext context)
        {
            // Measure the contents
            string content = context.Builder.ToString();
            context.Builder.Clear();
            var bounds = context.Measurer.Measure(content, context.Style.FontFamily, context.Style.Bold, context.Style.FontSize);

            // Return the span
            return new TextSpan(content, context.Style, bounds);
        }

        private static IStyle ParseColorStyle(SimpleTextLexer lexer, SimpleTextContext context)
        {
            var tracker = lexer.Track();
            while (lexer.Check(~TokenType.CloseBracket))
                lexer.Next();
            string color = lexer.GetTracked(tracker, false).Content.ToString().Trim();
            lexer.Branch(TokenType.CloseBracket);

            return new ColorStyleModifier.Style(context.Style, color, null);
        }
    }
}
