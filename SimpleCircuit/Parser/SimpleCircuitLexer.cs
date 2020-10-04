using System;
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
        public string Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCircuitLexer"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        public SimpleCircuitLexer(string input)
        {
            _input = input;
            Line = 1;
            Position = 1;
            _index = 0;
        }

        /// <summary>
        /// Resets the lexer to the start of the stream.
        /// </summary>
        public void Reset()
        {
            _index = 0;
            Line = 1;
            Position = 1;
        }

        /// <summary>
        /// Returns the next token.
        /// </summary>
        /// <param name="content">The content of the token.</param>
        /// <returns>The token type.</returns>
        public bool Next()
        {
            Type = TokenType.Whitespace;
            while (Type == TokenType.Whitespace || Type == TokenType.Comment)
            {
                if (_index >= _input.Length)
                {
                    Content = "";
                    Type = TokenType.EndOfContent;
                    return false;
                }
                var c = _input[_index];

                // Read a word
                if (char.IsLetter(c))
                {
                    ReadWord();
                    Content = _tokenBuilder.ToString();
                    Type = TokenType.Word;
                    return true;
                }

                // Read a number
                if (char.IsDigit(c))
                {
                    ReadNumber();
                    Content = _tokenBuilder.ToString();
                    Type = TokenType.Number;
                    return true;
                }

                switch (c)
                {
                    case '.':
                        Content = c.ToString();
                        _index++;
                        Position++;
                        Type = TokenType.Dot;
                        break;
                    case '-':
                        Content = c.ToString();
                        _index++;
                        Position++;
                        Type = TokenType.Dash;
                        break;
                    case '+':
                        Content = c.ToString();
                        _index++;
                        Position++;
                        Type = TokenType.Plus;
                        break;
                    case '*':
                        Content = c.ToString();
                        _index++;
                        Position++;
                        Type = TokenType.Times;
                        break;
                    case '/':
                        Content = c.ToString();
                        c = Store(c);
                        if (c == '/')
                        {
                            ReadComment();
                            Content = _tokenBuilder.ToString();
                            Type = TokenType.Comment;
                        }
                        else
                        {
                            Position++;
                            Content = "/";
                            Type = TokenType.Divide;
                        }
                        break;
                    case '(':
                    case '[':
                    case '<':
                        Content = c.ToString();
                        _index++;
                        Position++;
                        Type = TokenType.OpenBracket;
                        break;
                    case ')':
                    case ']':
                    case '>':
                        Content = c.ToString();
                        _index++;
                        Position++;
                        Type = TokenType.CloseBracket;
                        break;
                    case '=':
                        Content = c.ToString();
                        _index++;
                        Position++;
                        Type = TokenType.Equals;
                        break;
                    case '\r':
                    case '\n':
                        ReadNewline();
                        Content = _tokenBuilder.ToString();
                        Type = TokenType.Newline;
                        break;
                    case ' ':
                    case '\t':
                        ReadWhitespace();
                        Content = _tokenBuilder.ToString();
                        Type = TokenType.Whitespace;
                        break;
                    case '"':
                        ReadString();
                        Content = _tokenBuilder.ToString();
                        Type = TokenType.String;
                        break;
                    default:
                        throw new ParseException($"Unrecognized character '{c}'", Line, Position);
                }
            }
            return true;
        }

        /// <summary>
        /// Determines whether the current token if of the specified type and has the specified content.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the type and content matches; otherwise, <c>false</c>.
        /// </returns>
        public bool Is(TokenType type, string content = null)
        {
            if (Type != type)
                return false;
            if (content != null && string.CompareOrdinal(content, Content) != 0)
                return false;
            return true;
        }

        /// <summary>
        /// Checks for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="content">The content.</param>
        public void Check(SimpleCircuitLexer lexer, TokenType type, string content = null)
        {
            var error = Type != type;
            if (content != null && string.CompareOrdinal(content, Content) != 0)
                error = true;
            if (error)
                throw new ParseException($"Expected {(content != null ? content : $"a {type}")}", lexer.Line, lexer.Position);
            Next();
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
            if (c == '\r')
            {
                c = Store(c);
                if (c == '\n')
                    Store(c);
            }
            else
                Store(c);
            Position = 1;
            Line++;
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
                    throw new ParseException($"A number was detected but stops at a decimal point", Line, Position);
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
                    throw new ParseException($"A number was detected but stops at the exponential character", Line, Position);
                c = Store(c);
                if (c == '+' || c == '-')
                {
                    c = Store(c);
                    if (!char.IsDigit(c))
                        throw new ParseException($"A number was detected but stops at the exponential sign", Line, Position);
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
        private void ReadComment()
        {
            _tokenBuilder.Clear();
            var c = _input[_index];
            if (c == '/')
                c = Store(c);
            else
                throw new ParseException("Unrecognized comment statement", Line, Position);
            while (c != '\r' && c != '\n')
                c = Store(c);
        }
        private void ReadString()
        {
            _tokenBuilder.Clear();
            var c = _input[_index];
            c = Store(c);
            while (c != '\"' && c != '\0')
            {
                if (c == '\\')
                    c = Store(c);
                c = Store(c);
            }
            if (c == '\"')
                Store(c);
            else
                throw new ParseException("Lexer error: Expected closing quote", Line, Position);
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
