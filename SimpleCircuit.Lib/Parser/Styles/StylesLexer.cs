using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Parser.Styles
{
    [Flags]
    public enum TokenType
    {
        /// <summary>
        /// End of the content.
        /// </summary>
        EndOfContent = 0,

        /// <summary>
        /// A style (CSS) key.
        /// </summary>
        Key = 0x01,

        /// <summary>
        /// A string.
        /// </summary>
        String = 0x02,

        /// <summary>
        /// A parenthesis.
        /// </summary>
        Parenthesis = 0x04,

        /// <summary>
        /// A semicolon ';'.
        /// </summary>
        Semicolon = 0x08,

        /// <summary>
        /// A colon ':'.
        /// </summary>
        Colon = 0x10,

        /// <summary>
        /// Any character
        /// </summary>
        Any = 0x20,

        /// <summary>
        /// All token types.
        /// </summary>
        All = -1
    }

    public class StylesLexer(string text) : Lexer<TokenType>(text, text)
    {
        /// <summary>
        /// Gets or sets the diagnostic message handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

        /// <inheritdoc />
        public override bool Check(TokenType flags) => (Type & flags) != 0;

        protected override void ReadToken()
        {
            while (Char == ' ' || Char == '\t')
                ContinueTrivia();

            char c = Char;
            switch (c)
            {
                case '\0':
                    NextType = TokenType.EndOfContent;
                    break;

                case char l when char.IsLetter(l):
                    NextType = TokenType.Key;
                    ContinueToken();
                    while (char.IsLetterOrDigit(c = Char) || c == '-' || c == '_')
                        ContinueToken();
                    break;

                case '(':
                case ')':
                    NextType = TokenType.Parenthesis;
                    ContinueToken();
                    break;

                case '\'':
                case '"':
                    NextType = TokenType.String;
                    ContinueToken();
                    ContinueString(c);
                    break;

                case ';':
                    NextType = TokenType.Semicolon;
                    ContinueToken();
                    break;

                case ':':
                    NextType = TokenType.Colon;
                    ContinueToken();
                    break;

                default:
                    NextType = TokenType.Any;
                    ContinueToken();
                    break;
            }
        }

        private void ContinueString(char end)
        {
            char c = Char;
            while (c != end && c != '\0' && c != '\r' && c != '\n')
            {
                if (c == '\\')
                {
                    // Escape character
                    ContinueToken();
                }

                ContinueToken();
                c = Char;
            }
            if (c != end)
            {
                Diagnostics?.Post(NextToken, ErrorCodes.QuoteMismatch);
            }
            ContinueToken();
        }
    }
}
