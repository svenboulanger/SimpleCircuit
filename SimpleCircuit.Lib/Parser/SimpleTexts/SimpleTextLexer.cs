namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// A lexer for parsing simple texts/labels.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="SimpleTextLexer"/>.
    /// </remarks>
    /// <param name="text">The text.</param>
    public class SimpleTextLexer(string text) : Lexer<TokenType>(text, text)
    {
        /// <inheritdoc />
        public override bool Check(TokenType flags) => (Type & flags) != 0;

        /// <inheritdoc />
        protected override void ReadToken()
        {
            // There are no trivia in this lexer
            switch (Char)
            {
                case '\0':
                    NextType = TokenType.EndOfContent;
                    break;

                case '\\':
                    // Escaped sequence
                    NextType = TokenType.EscapedSequence;
                    ContinueToken();
                    switch (Char)
                    {
                        case 'n':
                            NextType = TokenType.Newline;
                            ContinueToken();
                            break;

                        case '_':
                        case '^':
                        case '\\':
                        case '{':
                        case '}':
                            NextType = TokenType.EscapedCharacter;
                            ContinueToken();
                            break;

                        default:
                            NextType = TokenType.EscapedSequence;
                            while (char.IsLetter(Char))
                                ContinueToken();
                            break;
                    }
                    break;

                case '_':
                    NextType = TokenType.Subscript;
                    ContinueToken();
                    break;

                case '^':
                    NextType = TokenType.Superscript;
                    ContinueToken();
                    break;

                case '{':
                    NextType = TokenType.OpenBracket;
                    ContinueToken();
                    break;

                case '}':
                    NextType = TokenType.CloseBracket;
                    ContinueToken();
                    break;

                default:
                    NextType = TokenType.Character;
                    ContinueToken();
                    break;
            }
        }
    }
}
