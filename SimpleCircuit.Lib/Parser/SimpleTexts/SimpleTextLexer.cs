using System;

namespace SimpleCircuit.Parser.SimpleTexts
{
    public class SimpleTextLexer : Lexer<TokenType>
    {
        /// <inheritdoc />
        public override bool Check(TokenType flags) => (Type & flags) != 0;

        /// <summary>
        /// Creates a new <see cref="SimpleTextLexer"/>.
        /// </summary>
        /// <param name="text">The text.</param>
        public SimpleTextLexer(string text)
            : base(text.AsMemory(), text)
        {
        }

        /// <inheritdoc />
        protected override void ReadToken()
        {
            // There are no trivia in this lexer
            switch (Char)
            {
                case '\0':
                    Type = TokenType.EndOfContent;
                    break;

                case '\\':
                    // Escape character
                    Type = TokenType.Escaped;
                    ContinueToken();
                    if (Char == 'n')
                        Type = TokenType.Newline;
                    ContinueToken();
                    break;

                case '_':
                    Type = TokenType.Subscript;
                    ContinueToken();
                    break;

                case '^':
                    Type = TokenType.Superscript;
                    ContinueToken();
                    break;

                case '{':
                    Type = TokenType.OpenBracket;
                    ContinueToken();
                    break;

                case '}':
                    Type = TokenType.CloseBracket;
                    ContinueToken();
                    break;

                default:
                    Type = TokenType.Character;
                    ContinueToken();
                    break;
            }
        }
    }
}
