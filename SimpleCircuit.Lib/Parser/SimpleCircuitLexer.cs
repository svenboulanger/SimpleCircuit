using SimpleCircuit.Diagnostics;
using System.IO;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A lexer for SimpleCircuit code.
    /// </summary>
    public class SimpleCircuitLexer : Lexer<TokenType>
    {
        /// <summary>
        /// Gets or sets the diagnostic message handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

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
        /// Creates a lexer for a file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The lexer, or <c>null</c> if the file doesn't exist.</returns>
        public static SimpleCircuitLexer FromFile(string filename)
        {
            // Expand the filename
            if (!Path.IsPathRooted(filename))
                filename = Path.Combine(Directory.GetCurrentDirectory(), filename);
            if (!File.Exists(filename))
                return null;

            // Read the file
            string contents;
            using (var sr = new StreamReader(filename))
                contents = sr.ReadToEnd();
            return new(contents, filename);
        }

        /// <summary>
        /// Creates a new <see cref="SimpleCircuitLexer"/>.
        /// </summary>
        /// <param name="code">The source.</param>
        /// <param name="line">The starting line number.</param>
        protected SimpleCircuitLexer(string code, string source, int line = 1)
            : base(code, source, line)
        {
        }

        /// <inheritdoc />
        public override bool Check(TokenType flags) => (Type & flags) != 0;

        /// <inheritdoc />
        protected override void ReadToken()
        {
            bool isNewLine = Column == 1;

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
                        NextType = TokenType.EndOfContent;
                        break;

                    case '-':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '-')
                            ContinueToken();
                        break;

                    case '+':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '+')
                            ContinueToken();
                        break;

                    case '=':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '=')
                            ContinueToken();
                        break;

                    case '.':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case ',':
                    case ':':
                    case '~':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        break;

                    case '?':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '?')
                            ContinueToken();
                        break;

                    case '<':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '<' || Char == '=')
                            ContinueToken();
                        break;

                    case '>':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '>' || Char == '=')
                            ContinueToken();
                        break;

                    case '|':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '|')
                            ContinueToken();
                        break;

                    case '&':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '&')
                            ContinueToken();
                        break;

                    case '!':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '=')
                            ContinueToken();
                        break;

                    case '*':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (isNewLine)
                        {
                            ContinueLineComment();
                            isTrivia = true;
                        }
                        break;

                    case '/':
                        NextType = TokenType.Punctuator;
                        ContinueToken();
                        if (Char == '/')
                        {
                            ContinueLineComment();
                            isTrivia = true;
                        }
                        break;

                    case '"':
                    case '\'':
                        NextType = TokenType.String;
                        ContinueToken();
                        ContinueString(c);
                        break;

                    case '\r':
                        NextType = TokenType.Newline;
                        ContinueToken();
                        if (Char == '\n')
                            ContinueToken();
                        Newline();
                        isTrivia = TryContinueLineContinuation();
                        break;

                    case '\n':
                        NextType = TokenType.Newline;
                        ContinueToken();
                        Newline();
                        isTrivia = TryContinueLineContinuation();
                        break;

                    case '\u2190': // Left arrow
                    case '\u2191': // Up arrow
                    case '\u2192': // Right arrow
                    case '\u2193': // Down arrow
                    case '\u2196': // North-west arrow
                    case '\u2197': // North-east arrow
                    case '\u2198': // South-east arrow
                    case '\u2199': // South-west arrow
                        NextType = TokenType.Arrow;
                        ContinueToken();
                        break;

                    case char w when char.IsLetter(w) || w == '_':
                        NextType = TokenType.Word;
                        ContinueWord();
                        break;

                    case char d when char.IsDigit(d):
                        NextType = TokenType.Number;
                        ContinueNumber();
                        break;

                    default:
                        NextType = TokenType.Unknown;
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
            // Whole number mantissa
            while (char.IsDigit(Char))
                ContinueToken(); // digit

            // Decimal point
            if (Char == '.')
            {
                NextType = TokenType.Number;

                // '.'
                ContinueToken(); // '.'

                // Fraction (optional)
                while (char.IsDigit(Char))
                    ContinueToken(); // number
            }

            // 'e' or 'E'
            if (Char == 'e' || Char == 'E')
            {
                // '+' or '-'
                char la = LookAhead();
                if (la == '+' || la == '-')
                {
                    // Number
                    if (char.IsDigit(LookAhead(2)))
                    {
                        NextType = TokenType.Number;
                        ContinueToken(); // 'e' or 'E'
                        ContinueToken(); // '+' or '-'
                        ContinueToken(); // number
                        while (char.IsDigit(Char))
                            ContinueToken();
                    }
                }

                // digit
                else if (char.IsDigit(la))
                {
                    NextType = TokenType.Number;
                    ContinueToken(); // 'e' or 'E'
                    ContinueToken(); // number
                    while (char.IsDigit(Char))
                        ContinueToken();
                }
            }

            // Spice modifiers (optional)
            if (char.IsLetter(Char))
            {
                NextType = TokenType.Number;
                ContinueToken();
                while (char.IsLetterOrDigit(Char) || Char == '_')
                    ContinueToken();
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

        private bool TryContinueLineContinuation()
        {
            // Try to get to a '+'
            int la = 0;
            char c = LookAhead(la);
            while (c == ' ' || c == '\t')
            {
                la++;
                c = LookAhead(la);
            }
            if (c == '+')
            {
                while (la >= 0)
                {
                    ContinueTrivia();
                    la--;
                }
                while ((c = Char) == ' ' || c == '\t')
                    ContinueTrivia();
                return true;
            }
            return false;
        }
    }
}
