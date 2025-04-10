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
    public class VariantLexer(string text) : Lexer<TokenType>(text, text)
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
                    NextType = TokenType.EndOfContent;
                    break;

                case '(':
                    NextType = TokenType.OpenBracket;
                    ContinueToken();
                    break;

                case ')':
                    NextType = TokenType.CloseBracket;
                    ContinueToken();
                    break;

                case '|':
                    NextType = TokenType.Or;
                    ContinueToken();
                    if (Char == '|')
                        ContinueToken();
                    break;

                case '&':
                    NextType = TokenType.And;
                    ContinueToken();
                    if (Char == '&')
                        ContinueToken();
                    break;

                case '!':
                    NextType = TokenType.Not;
                    ContinueToken();
                    break;

                case 'a':
                    NextType = TokenType.Variant;
                    ContinueToken();
                    if (Char == 'n')
                    {
                        ContinueToken();
                        if (Char == 'd')
                        {
                            ContinueToken();
                            NextType = TokenType.And;
                        }
                    }
                    while (char.IsLetterOrDigit(c = Char) || c == '_' || c == '-')
                    {
                        NextType = TokenType.Variant;
                        ContinueToken();
                    }
                    break;

                case 'o':
                    NextType = TokenType.Variant;
                    ContinueToken();
                    if (Char == 'r')
                    {
                        ContinueToken();
                        NextType = TokenType.Or;
                    }
                    while (char.IsLetterOrDigit(c = Char) || c == '_' || c == '-')
                    {
                        NextType = TokenType.Variant;
                        ContinueToken();
                    }
                    break;

                case 'n':
                    NextType = TokenType.Variant;
                    ContinueToken();
                    if (Char == 'o')
                    {
                        ContinueToken();
                        if (Char == 't')
                        {
                            NextType = TokenType.Not;
                            ContinueToken();
                        }
                    }
                    while (char.IsLetterOrDigit(c = Char) || c == '_' || c == '-')
                    {
                        NextType = TokenType.Variant;
                        ContinueToken();
                    }
                    break;

                case char l when char.IsLetter(l):
                    NextType = TokenType.Variant;
                    ContinueToken();
                    while (char.IsLetterOrDigit(c = Char) || c == '_' || c == '-')
                        ContinueToken();
                    break;

                default:
                    NextType = TokenType.Variant;
                    ContinueToken();
                    break;
            }
        }
    }
}
