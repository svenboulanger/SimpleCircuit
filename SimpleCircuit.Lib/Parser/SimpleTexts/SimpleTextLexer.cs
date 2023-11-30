using System;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// A lexer for parsing simple texts/labels.
    /// </summary>
    public class SimpleTextLexer : Lexer<TokenType>
    {
        /// <summary>
        /// Creates a new <see cref="SimpleTextLexer"/>.
        /// </summary>
        /// <param name="text">The text.</param>
        public SimpleTextLexer(string text)
            : base(text.AsMemory(), text)
        {
        }

        /// <inheritdoc />
        public override bool Check(TokenType flags) => (Type & flags) != 0;

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
                    // Escaped sequence
                    Type = TokenType.Escaped;
                    ContinueToken();
                    if (Char == 'n')
                    {
                        Type = TokenType.Newline;
                        ContinueToken();
                    }
                    else if (Char == '\\')
                    {
                        Type = TokenType.Slash;
                        ContinueToken();
                    }
                    else
                    {
                        while (char.IsLetter(Char))
                        {
                            Type = TokenType.Escaped;
                            ContinueToken();
                        }
                    }
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
