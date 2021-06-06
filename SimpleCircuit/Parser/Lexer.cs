namespace SimpleCircuit.Parser
{
    /// <summary>
    /// The lexer used for SimpleCircuit scripts.
    /// </summary>
    public class Lexer
    {
        private int _index = 0, _length = 0;
        private readonly string _input;

        /// <summary>
        /// Gets the current character.
        /// </summary>
        private char Char => _index >= _input.Length ? '\0' : _input[_index];
        
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
        public int Column { get; private set; }

        /// <summary>
        /// Gets the type of the current token.
        /// </summary>
        /// <value>
        /// The type of the current token.
        /// </value>
        public TokenType Type { get; private set; }

        /// <summary>
        /// Gets the content of the current token.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content => _input.Substring(_index - _length, _length);

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        public Lexer(string input)
        {
            _input = input;
            Reset();

            // Read the first token
            Next();
        }

        /// <summary>
        /// Resets the lexer to the start of the stream.
        /// </summary>
        public void Reset()
        {
            _index = 0;
            _length = 0;
            Line = 1;
            Column = 1;
        }

        /// <summary>
        /// Read a new token.
        /// </summary>
        public void Next()
        {
            _length = 0;
            if (_index >= _input.Length)
            {
                Type = TokenType.EndOfContent;
                return;
            }

            char c = Char;
            switch (c)
            {
                case '.':
                    Type = TokenType.Dot;
                    Continue();
                    break;

                case '-':
                    Type = TokenType.Dash;
                    Continue();
                    break;

                case '+':
                    Type = TokenType.Plus;
                    Continue();
                    break;

                case '*':
                    Type = TokenType.Times;
                    Continue();
                    break;

                case '/':
                    Type = TokenType.Divide;
                    Continue();
                    if (Char == '/')
                    {
                        Type = TokenType.Comment;
                        Continue();
                        while ((c = Char) != '\r' && c != '\n' && c != '\0')
                            Continue();
                    }
                    break;

                case ' ':
                case '\t':
                    Type = TokenType.Whitespace;
                    Continue();
                    while ((c = Char) == ' ' || c == '\t')
                        Continue();
                    break;

                case '(':
                    Type = TokenType.OpenParenthesis;
                    Continue();
                    break;

                case ')':
                    Type = TokenType.CloseParenthesis;
                    Continue();
                    break;

                case '[':
                    Type = TokenType.OpenIndex;
                    Continue();
                    break;

                case ']':
                    Type = TokenType.CloseIndex;
                    Continue();
                    break;

                case '<':
                    Type = TokenType.OpenBeak;
                    Continue();
                    break;

                case '>':
                    Type = TokenType.CloseBeak;
                    Continue();
                    break;

                case '=':
                    Type = TokenType.Equals;
                    Continue();
                    break;

                case '?':
                    Type = TokenType.Question;
                    Continue();
                    break;

                case '\'':
                case '"':
                    Type = TokenType.String;
                    char quote = Char;
                    Continue();
                    while ((c = Char) != quote && c != '\0' && c != '\r' && c != '\n')
                    {
                        // Escape character
                        if (c == '\\')
                            Continue();
                        Continue();
                    }
                    if (c == '\0')
                        throw new ParseException(this, $"Unexpected end of string");
                    Continue();
                    break;

                case char digit when char.IsDigit(digit):
                    Type = TokenType.Number;
                    Continue();
                    while (char.IsDigit(Char))
                        Continue();

                    // Fractions
                    if (Char == '.')
                    {
                        Continue();
                        while (char.IsDigit(Char))
                            Continue();
                    }

                    // Exponent
                    if ((c = Char) == 'e' || c == 'E')
                    {
                        Continue();
                        if ((c = Char) == '+' || c == '-')
                            Continue();
                        while (char.IsDigit(Char))
                            Continue();
                    }
                    break;

                case '\r':
                    Type = TokenType.Newline;
                    Continue();
                    if (Char == '\n')
                        Continue();
                    Newline();
                    break;

                case '\n':
                    Type = TokenType.Newline;
                    Continue();
                    Newline();
                    break;

                case char letter when char.IsLetter(letter):
                    Type = TokenType.Word;
                    Continue();
                    while (char.IsLetterOrDigit(c = Char) || c == '_')
                        Continue();
                    break;

                default:
                    Type = TokenType.Unknown;
                    Continue();
                    break;
            }
        }

        /// <summary>
        /// Skips any token of the specified type.
        /// </summary>
        /// <param name="flags">The token types to skip.</param>
        public void SkipWhile(TokenType flags)
        {
            while ((Type & flags) != 0)
                Next();
        }

        private void Continue()
        {
            if (_index < _input.Length)
            {
                _index++;
                _length++;
                Column++;
            }
        }
        private void Newline()
        {
            Line++;
            Column = 1;
        }
    }
}
