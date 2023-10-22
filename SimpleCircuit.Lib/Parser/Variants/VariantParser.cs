namespace SimpleCircuit.Parser.Variants
{
    /// <summary>
    /// Methods for parsing variant expressions.
    /// </summary>
    public static class VariantParser
    {
        /// <summary>
        /// Parses a variant combination.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The context.</param>
        /// <returns>Returns <c>true</c> if the variant expression is valid; otherwise, <c>false</c>.</returns>
        public static bool Parse(VariantLexer lexer, IVariantContext context)
        {
            return Or(lexer, context);
        }

        private static bool Or(VariantLexer lexer, IVariantContext context)
        {
            bool result = And(lexer, context);
            while (lexer.Branch(TokenType.Or))
                result |= And(lexer, context);
            return result;
        }
        private static bool And(VariantLexer lexer, IVariantContext context)
        {
            bool result = Not(lexer, context);
            while (lexer.Branch(TokenType.And))
                result &= Not(lexer, context);
            return result;
        }

        private static bool Not(VariantLexer lexer, IVariantContext context)
        {
            if (lexer.Branch(TokenType.Not))
                return !Not(lexer, context);
            return Variant(lexer, context);
        }

        private static bool Variant(VariantLexer lexer, IVariantContext context)
        {
            if (lexer.Branch(TokenType.Variant, out var token))
                return context.Contains(token.Content.ToString());
            else if (lexer.Branch(TokenType.OpenBracket))
            {
                var result = Or(lexer, context);
                if (!lexer.Branch(TokenType.CloseBracket))
                {
                    lexer.Skip(TokenType.All);
                    return false;
                }
                return result;
            }

            lexer.Skip(TokenType.All);
            return false;
        }
    }
}
