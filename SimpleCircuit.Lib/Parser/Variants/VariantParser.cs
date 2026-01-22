namespace SimpleCircuit.Parser.Variants;

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
        return Or(lexer, context, true);
    }

    private static bool Or(VariantLexer lexer, IVariantContext context, bool relevant)
    {
        bool result = And(lexer, context, relevant);
        while (lexer.Branch(TokenType.Or))
        {
            if (result)
                And(lexer, context, false);
            else
                result |= And(lexer, context, relevant);
        }
        return result;
    }
    private static bool And(VariantLexer lexer, IVariantContext context, bool relevant)
    {
        bool result = Not(lexer, context, relevant);
        while (lexer.Branch(TokenType.And))
        {
            if (!result)
                Not(lexer, context, false);
            else
                result &= Not(lexer, context, relevant);
        }
        return result;
    }

    private static bool Not(VariantLexer lexer, IVariantContext context, bool relevant)
    {
        if (lexer.Branch(TokenType.Not))
            return !Not(lexer, context, relevant);
        return Variant(lexer, context, relevant);
    }

    private static bool Variant(VariantLexer lexer, IVariantContext context, bool relevant)
    {
        if (lexer.Branch(TokenType.Variant, out var token))
        {
            if (relevant)
                return context.Contains(token.Content.ToString());
            else
                return false;
        }

        else if (lexer.Branch(TokenType.OpenBracket))
        {
            var result = Or(lexer, context, relevant);
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
