namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A lexer for SimpleCircuit code.
    /// </summary>
    public class SimpleCircuitLexer : Lexer<TokenType>
    {
        /// <summary>
        /// Creates a new lexer for a string.
        /// </summary>
        /// <param name="netlist">The netlist.</param>
        /// <returns>The lexer.</returns>
        public static SimpleCircuitLexer FromString(string netlist)
            => new(netlist);

        /// <summary>
        /// Creates a new <see cref="SimpleCircuitLexer"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="line">The starting line number.</param>
        private SimpleCircuitLexer(string source, int line = 1)
            : base(source, line)
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

                    case char w when char.IsLetter(w):
                        Type = TokenType.Word;
                        ContinueWord();
                        break;

                    case char d when char.IsDigit(d):
                        Type = TokenType.Number;
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
                ContinueToken();
                while (char.IsDigit(c))
                {
                    ContinueToken();
                    c = Char;
                }
            }

            if (c == 'e' || c == 'E')
            {
                // Exponential notation
                ContinueToken();
                c = Char;
                if (c == '+' || c == '-')
                {
                    ContinueToken();
                    c = Char;
                }
                while (char.IsDigit(c))
                {
                    ContinueToken();
                    c = Char;
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
                throw new ParserException(this, "Quote mismatch");
            ContinueToken();
        }
    }
}
