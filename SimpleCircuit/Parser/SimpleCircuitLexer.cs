using System.Text;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// The lexer used for SimpleCircuit scripts.
    /// </summary>
    public class SimpleCircuitLexer
    {
        private int _index = 0;
        private readonly string _input;
        private readonly StringBuilder _tokenBuilder = new StringBuilder(16);

        /// <summary>
        /// Gets the line number.
        /// </summary>
        /// <value>
        /// The line.
        /// </value>
        public int Line { get; private set; }

        /// <summary>
        /// Gets the position in the line.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public int Position { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCircuitLexer"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        public SimpleCircuitLexer(string input)
        {
            _input = input;
        }

        /// <summary>
        /// Resets the lexer to the start of the stream.
        /// </summary>
        public void Reset()
        {
            _index = 0;
            Line = 0;
            Position = 0;
        }

        /// <summary>
        /// Returns the next token.
        /// </summary>
        /// <param name="content">The content of the token.</param>
        /// <returns>The token type.</returns>
        public TokenType Next(out string content)
        {
            if (_index >= _input.Length)
            {
                content = "";
                return TokenType.EndOfContent;
            }
            var c = _input[_index];

            // Read a word
            if (char.IsLetter(c))
            {
                ReadWord();
                content = _tokenBuilder.ToString();
                return TokenType.Word;
            }
            
            // Read a number
            if (char.IsDigit(c))
            {
                ReadNumber();
                content = _tokenBuilder.ToString();
                return TokenType.Number;
            }

            switch (c)
            {
                case '.':
                    content = c.ToString();
                    _index++;
                    Position++;
                    return TokenType.Dot;
                case '-':
                    content = c.ToString();
                    _index++;
                    Position++;
                    return TokenType.Dash;
                case '+':
                    content = c.ToString();
                    _index++;
                    Position++;
                    return TokenType.Plus;
                case '(':
                case '[':
                    content = c.ToString();
                    _index++;
                    Position++;
                    return TokenType.OpenBracket;
                case ')':
                case ']':
                    content = c.ToString();
                    _index++;
                    Position++;
                    return TokenType.CloseBracket;
                case '=':
                    content = c.ToString();
                    _index++;
                    Position++;
                    return TokenType.Equals;
                case '\r':
                case '\n':
                    ReadNewline();
                    content = _tokenBuilder.ToString();
                    return TokenType.Newline;
                case ' ':
                case '\t':
                    ReadWhitespace();
                    content = _tokenBuilder.ToString();
                    return TokenType.Whitespace;
                default:
                    throw new LexerException($"Unrecognized character '{c}' at line {Line}, position {Position}.");
            }
        }

        private void ReadWord()
        {
            _tokenBuilder.Clear();
            var c = _input[_index];
            while (char.IsLetterOrDigit(c))
                c = Store(c);
        }
        private void ReadNewline()
        {
            _tokenBuilder.Clear();
            var c = _input[_index];
            var last = '?';
            while (c == '\r' || c == '\n')
            {
                c = Store(c);

                // Detect \r\n, \r and \n as new lines
                if (c == '\n' && last == '\r' || last == '?' || last == c)
                    Line++;
                if (_index >= _input.Length)
                {
                    Position = 0;
                    return;
                }
                last = c;
                c = _input[_index];
            }
            Position = 0;
        }
        private void ReadNumber()
        {
            _tokenBuilder.Clear();
            var c = _input[_index];

            // Read mantissa
            while (c >= '0' && c <= '9')
            {
                c = Store(c);
                if (c == '\0')
                    return;
            }

            // Read a dot
            if (c == '.')
            {
                c = Store(c);
                if (!char.IsDigit(c))
                    throw new LexerException($"A number was detected but stops at a decimal point at line {Line}, position {Position}.");
                c = Store(c);
                while (c >= '0' && c <= '9')
                {
                    c = Store(c);
                    if (c == '\0')
                        return;
                }
            }

            // Read an exponential part
            if (c == 'e' || c == 'E')
            {
                c = Store(c);
                if (!char.IsDigit(_input[_index]) && _input[_index] != '+' && _input[_index] != '-')
                    throw new LexerException($"A number was detected but stops at the exponential character at line {Line}, position {Position}.");
                c = Store(c);
                if (c == '+' || c == '-')
                {
                    c = Store(c);
                    if (!char.IsDigit(c))
                        throw new LexerException($"A number was detected but stops at the exponential sign at line {Line}, position {Position}.");
                }
                while (c >= '0' && c <= '9')
                {
                    c = Store(c);
                    if (c == '\0')
                        return;
                }
            }
        }
        private void ReadWhitespace()
        {
            _tokenBuilder.Clear();
            var c = _input[_index];
            while (c == ' ' || c == '\t')
                c = Store(c);
        }
        private char Store(char c)
        {
            _tokenBuilder.Append(c);
            _index++;
            Position++;
            if (_index >= _input.Length)
                return '\0';
            return _input[_index];
        }
    }
}
