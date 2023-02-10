using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A lexer for SimpleCircuit code.
    /// </summary>
    public class SimpleCircuitLexer : Lexer<TokenType>
    {
        private const double _isqr2 = 0.70710678118;

        /// <summary>
        /// Gets or sets the diagnostic message handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

        /// <summary>
        /// Gets the orientation of the last encountered arrow.
        /// </summary>
        public Vector2 ArrowOrientation { get; private set; }

        /// <summary>
        /// Creates a new lexer for a string.
        /// </summary>
        /// <param name="netlist">The netlist.</param>
        /// <param name="source">The source.</param>
        /// <param name="line">The line number.</param>
        /// <returns>The lexer.</returns>
        public static SimpleCircuitLexer FromString(string netlist, string source = null, int line = 1)
            => new(netlist, source, line);

        /// <summary>
        /// Creates a new <see cref="SimpleCircuitLexer"/>.
        /// </summary>
        /// <param name="code">The source.</param>
        /// <param name="line">The starting line number.</param>
        private SimpleCircuitLexer(string code, string source, int line = 1)
            : base(code, source, line)
        {
        }

        /// <inheritdoc />
        public override bool Check(TokenType flags) => (Type & flags) != 0;

        /// <inheritdoc />
        protected override void ReadToken()
        {
            // White spaces are trivia
            char c = Char;
            while (c == ' ' || c == '\t')
            {
                ContinueTrivia();
                c = Char;
            }

            // Tokens
            bool isTrivia = true;
            while (isTrivia)
            {
                isTrivia = false;
                c = Char;
                switch (c)
                {
                    case '\0':
                        Type = TokenType.EndOfContent;
                        break;

                    case '.':
                        Type = TokenType.Dot;
                        ContinueToken();
                        break;

                    case '-':
                        Type = TokenType.Dash;
                        ContinueToken();
                        break;

                    case '+':
                        Type = TokenType.Plus;
                        ContinueToken();
                        break;

                    case '*':
                        Type = TokenType.Times;
                        ContinueToken();
                        break;

                    case '/':
                        Type = TokenType.Divide;
                        ContinueToken();
                        if (Char == '/')
                        {
                            ContinueLineComment();
                            isTrivia = true;
                        }
                        break;

                    case '"':
                    case '\'':
                        Type = TokenType.String;
                        ContinueToken();
                        ContinueString(c);
                        break;

                    case '\r':
                        Type = TokenType.Newline;
                        ContinueToken();
                        if (Char == '\n')
                            ContinueToken();
                        Newline();
                        break;

                    case '\n':
                        Type = TokenType.Newline;
                        ContinueToken();
                        Newline();
                        break;

                    case '(':
                        Type = TokenType.OpenParenthesis;
                        ContinueToken();
                        break;

                    case ')':
                        Type = TokenType.CloseParenthesis;
                        ContinueToken();
                        break;

                    case '[':
                        Type = TokenType.OpenIndex;
                        ContinueToken();
                        break;

                    case ']':
                        Type = TokenType.CloseIndex;
                        ContinueToken();
                        break;

                    case '<':
                        Type = TokenType.OpenBeak;
                        ContinueToken();
                        break;

                    case '>':
                        Type = TokenType.CloseBeak;
                        ContinueToken();
                        break;

                    case '=':
                        Type = TokenType.Equals;
                        ContinueToken();
                        break;

                    case ',':
                        Type = TokenType.Comma;
                        ContinueToken();
                        break;

                    case '?':
                        Type = TokenType.Question;
                        ContinueToken();
                        break;

                    case '\u2190': // Left arrow
                        ArrowOrientation = new(-1, 0);
                        Type = TokenType.Arrow;
                        ContinueToken();
                        break;

                    case '\u2191': // Up arrow
                        ArrowOrientation = new(0, -1);
                        Type = TokenType.Arrow;
                        ContinueToken();
                        break;

                    case '\u2192': // Right arrow
                        ArrowOrientation = new(1, 0);
                        Type = TokenType.Arrow;
                        ContinueToken();
                        break;

                    case '\u2193': // Down arrow
                        ArrowOrientation = new(0, 1);
                        Type = TokenType.Arrow;
                        ContinueToken();
                        break;

                    case '\u2196': // North-west arrow
                        ArrowOrientation = new(-_isqr2, -_isqr2);
                        Type = TokenType.Arrow;
                        ContinueToken();
                        break;

                    case '\u2197': // North-east arrow
                        ArrowOrientation = new(_isqr2, -_isqr2);
                        Type = TokenType.Arrow;
                        ContinueToken();
                        break;

                    case '\u2198': // South-east arrow
                        ArrowOrientation = new(_isqr2, _isqr2);
                        Type = TokenType.Arrow;
                        ContinueToken();
                        break;

                    case '\u2199': // South-west arrow
                        ArrowOrientation = new(-_isqr2, _isqr2);
                        Type = TokenType.Arrow;
                        ContinueToken();
                        break;

                    case char w when char.IsLetter(w):
                        Type = TokenType.Word;
                        ContinueWord();
                        break;

                    case char d when char.IsDigit(d):
                        Type = TokenType.Integer;
                        ContinueNumber();
                        break;

                    default:
                        Type = TokenType.Unknown;
                        ContinueToken();
                        break;
                }
            }
        }

        private void ContinueLineComment()
        {
            char c = Char;
            while (c != '\r' && c != '\n' && c != '\0')
            {
                ContinueTrivia();
                c = Char;
            }
        }

        private void ContinueWord()
        {
            char c = Char;
            while (char.IsLetterOrDigit(c) || c == '_')
            {
                ContinueToken();
                c = Char;
            }
        }

        private void ContinueNumber()
        {
            char c = Char;
            while (char.IsDigit(c))
            {
                ContinueToken();
                c = Char;
            }

            if (c == '.')
            {
                // Fraction
                Type = TokenType.Number;
                ContinueToken();
                c = Char;
                while (char.IsDigit(c))
                {
                    ContinueToken();
                    c = Char;
                }
            }

            if (c == 'e' || c == 'E')
            {
                // Special case: if the type is integer at this point then we can migrate to a word
                if (Type == TokenType.Integer)
                    Type = TokenType.Word;

                // Exponential notation
                ContinueToken();
                c = Char;
                if (c == '+' || c == '-')
                {
                    // No ambiguity anymore!
                    Type = TokenType.Number;
                    ContinueToken();
                    c = Char;
                }

                if (!char.IsDigit(c))
                    ContinueWord();
                else
                {
                    while (char.IsDigit(c))
                    {
                        ContinueToken();
                        c = Char;
                    }
                }
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
                var loc = new TextLocation(Line, Column);
                Diagnostics?.Post(new Token(Source, new(loc, loc), Content), ErrorCodes.QuoteMismatch);
            }
            ContinueToken();
        }
    }
}
