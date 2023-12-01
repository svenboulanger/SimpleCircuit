using System;

namespace SimpleCircuit.Parser.Variants
{
    /// <summary>
    /// A lexer for parsing variant combinations.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="VariantLexer"/>.
    /// </remarks>
    /// <param name="text">The text.</param>
    public class VariantLexer(string text) : Lexer<TokenType>(text.AsMemory(), text)
    {

        /// <inheritdoc />
        public override bool Check(TokenType flags) => (Type & flags) != 0;

        /// <inheritdoc />
        protected override void ReadToken()
        {
            while (Char == ' ')
                ContinueTrivia();

            char c;
            switch (Char)
            {
                case '\0':
                    Type = TokenType.EndOfContent;
                    break;

                case '(':
                    Type = TokenType.OpenBracket;
                    ContinueToken();
                    break;

                case ')':
                    Type = TokenType.CloseBracket;
                    ContinueToken();
                    break;

                case '|':
                    Type = TokenType.Or;
                    ContinueToken();
                    if (Char == '|')
                        ContinueToken();
                    break;

                case '&':
                    Type = TokenType.And;
                    ContinueToken();
                    if (Char == '&')
                        ContinueToken();
                    break;

                case '!':
                    Type = TokenType.Not;
                    ContinueToken();
                    break;

                case 'a':
                    Type = TokenType.Variant;
                    ContinueToken();
                    if (Char == 'n')
                    {
                        ContinueToken();
                        if (Char == 'd')
                        {
                            ContinueToken();
                            Type = TokenType.And;
                        }
                    }
                    while (char.IsLetterOrDigit(c = Char) || c == '_' || c == '-')
                    {
                        Type = TokenType.Variant;
                        ContinueToken();
                    }
                    break;

                case 'o':
                    Type = TokenType.Variant;
                    ContinueToken();
                    if (Char == 'r')
                    {
                        ContinueToken();
                        Type = TokenType.Or;
                    }
                    while (char.IsLetterOrDigit(c = Char) || c == '_' || c == '-')
                    {
                        Type = TokenType.Variant;
                        ContinueToken();
                    }
                    break;

                case 'n':
                    Type = TokenType.Variant;
                    ContinueToken();
                    if (Char == 'o')
                    {
                        ContinueToken();
                        if (Char == 't')
                        {
                            Type = TokenType.Not;
                            ContinueToken();
                        }
                    }
                    while (char.IsLetterOrDigit(c = Char) || c == '_' || c == '-')
                    {
                        Type = TokenType.Variant;
                        ContinueToken();
                    }
                    break;

                case char l when char.IsLetter(l):
                    Type = TokenType.Variant;
                    ContinueToken();
                    while (char.IsLetterOrDigit(c = Char) || c == '_' || c == '-')
                        ContinueToken();
                    break;

                default:
                    Type = TokenType.Variant;
                    ContinueToken();
                    break;
            }
        }
    }
}
