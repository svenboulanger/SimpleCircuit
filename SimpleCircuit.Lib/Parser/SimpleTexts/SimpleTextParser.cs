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
            var result = new MultilineSpan(context.FontSize * context.LineSpacing, context.Align);

            // Parse lines
            while (lexer.Type != TokenType.EndOfContent)
            {
                result.Add(ParseLine(lexer, context));
                lexer.Branch(TokenType.Newline);
            }
            return result;
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
            if (lexer.Branch(TokenType.Subscript))
            {
                var s = new SubscriptSuperscriptSpan(result, 0.5 * context.FontSize)
                {
                    Margin = new(0, 0.05 * context.FontSize)
                };
                if (lexer.Branch(TokenType.OpenBracket))
                {
                    s.Sub = ParseBlockSegment(lexer, context);
                    lexer.Branch(TokenType.CloseBracket);
                }
                else
                    s.Sub = ParseLine(lexer, context);
                if (lexer.Branch(TokenType.Superscript))
                {
                    if (lexer.Branch(TokenType.OpenBracket))
                    {
                        s.Super = ParseBlockSegment(lexer, context);
                        lexer.Branch(TokenType.CloseBracket);
                    }
                    else
                        s.Super = ParseLine(lexer, context);
                }
                return s;
            }
            else if (lexer.Branch(TokenType.Superscript))
            {
                var s = new SubscriptSuperscriptSpan(result, 0.5 * context.FontSize)
                {
                    Margin = new(0, 0.05 * context.FontSize)
                };
                if (lexer.Branch(TokenType.OpenBracket))
                {
                    s.Super = ParseBlockSegment(lexer, context);
                    lexer.Branch(TokenType.CloseBracket);
                }
                else
                    s.Super = ParseLine(lexer, context);
                if (lexer.Branch(TokenType.Subscript))
                {
                    if (lexer.Branch(TokenType.OpenBracket))
                    {
                        s.Sub = ParseBlockSegment(lexer, context);
                        lexer.Branch(TokenType.CloseBracket);
                    }
                    else
                        s.Sub = ParseLine(lexer, context);
                }
                return s;
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
                case TokenType.Escaped:
                    // Check for an escape sequence
                    switch (lexer.Content.ToString())
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
                                context.Builder.Append("\\overline");
                                ContinueText(lexer, context);
                                return CreateTextSpan(context);
                            }

                        default:
                            context.Builder.Append(lexer.Content.ToString());
                            lexer.Next();
                            ContinueText(lexer, context);
                            return CreateTextSpan(context);
                    }

                default:
                    context.Builder.Append(lexer.Content.ToString());
                    lexer.Next();
                    ContinueText(lexer, context);
                    return CreateTextSpan(context);
            }
        }
        private static void ContinueText(SimpleTextLexer lexer, SimpleTextContext context)
        {
            while (lexer.Check(TokenType.Character | TokenType.Slash))
            {
                if (lexer.Check(TokenType.Slash))
                    context.Builder.Append('\\');
                else
                    context.Builder.Append(lexer.Content.ToString());
                lexer.Next();
            }
        }
        private static ISpan CreateOverline(ISpan @base, SimpleTextContext context)
        {
            double margin = context.FontSize * 0.05;
            double thickness = context.FontSize * 0.1;

            var element = context.Document.CreateElement("path", SvgDrawing.Namespace);
            element.SetAttribute("style", $"stroke-width:{thickness.ToCoordinate()}pt;fill:none;");
            context.Parent.AppendChild(element);

            return new OverlineSpan(element, @base, margin, thickness);
        }
        private static ISpan CreateTextSpan(SimpleTextContext context)
        {
            // Measure the contents
            string content = context.Builder.ToString();
            context.Builder.Clear();
            var bounds = context.Measurer.Measure(content, context.FontSize);

            // Create the XML element
            var element = context.Document.CreateElement("tspan", SvgDrawing.Namespace);
            element.SetAttribute("style", $"font-family:{context.Measurer.FontFamily};font-size:{context.FontSize.ToCoordinate()}pt;");
            element.InnerXml = content;
            context.Text.AppendChild(element);

            // Return the span
            return new TextSpan(element, bounds);
        }
    }
}
