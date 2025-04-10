using System;
using System.IO;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// An abstract class for a lexer.
    /// </summary>
    /// <typeparam name="T">The token type.</typeparam>
    public abstract class Lexer<T> : ILexer<T>
    {
        private readonly string _contents;
        private int _triviaLine, _triviaColumn, _triviaIndex;
        private int _tokenLine, _tokenColumn, _tokenIndex;
        private int _line, _column, _index;
        private int _lastTriviaIndex, _lastTokenIndex;

        /// <inheritdoc />
        public string Source { get; }

        /// <inheritdoc/>
        public ReadOnlyMemory<char> NextContent => _contents.AsMemory(_tokenIndex, _index - _tokenIndex);

        /// <inheritdoc />
        public ReadOnlyMemory<char> Content { get; private set; }

        /// <inheritdoc />
        public Token NextToken => new(new(Source, _tokenLine, _tokenColumn), NextContent);

        /// <inheritdoc />
        public Token Token { get; private set; }

        /// <inheritdoc />
        public T NextType { get; protected set; }

        /// <inheritdoc />
        public T Type { get; private set; }

        /// <inheritdoc />
        public bool NextHasTrivia => _triviaIndex < _tokenIndex;

        /// <inheritdoc />
        public bool HasTrivia { get; private set; }

        /// <summary>
        /// Gets the current character.
        /// </summary>
        protected char Char
        {
            get
            {
                if (_index >= _contents.Length)
                    return '\0';
                return _contents[_index];
            }
        }

        /// <summary>
        /// Gets the column of the current character.
        /// </summary>
        protected int Column => _column;

        /// <summary>
        /// Gets the line of the current character.
        /// </summary>
        protected int Line => _line;

        /// <summary>
        /// Creates a new <see cref="Lexer{T}"/>.
        /// </summary>
        /// <param name="reader">The text reader.</param>
        /// <param name="source">The source.</param>
        /// <param name="line">The initial line number.</param>
        protected Lexer(TextReader reader, string source, int line = 1)
        {
            Source = source;
            using (reader)
            {
                _contents = reader.ReadToEnd();
            }
            _line = line;
            _column = 1;
            Next();
        }


        /// <summary>
        /// Creates a new <see cref="Lexer{T}"/>.
        /// </summary>
        /// <param name="netlist">The source.</param>
        /// <param name="source">The source.</param>
        /// <param name="line">The initial line number.</param>
        protected Lexer(string netlist, string source = null, int line = 1)
        {
            Source = source;
            _contents = netlist;
            _line = line;
            _column = 1;
            Next();
            Next();
        }

        /// <summary>
        /// Looks ahead some characters.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>Returns the character in the future.</returns>
        protected char LookAhead(int offset = 1)
        {
            int index = _index + offset;
            if (index >= _contents.Length)
                return '\0';
            return _contents[index];
        }

        /// <inheritdoc />
        public void Next()
        {
            Token = NextToken;
            Type = NextType;
            Content = NextContent;
            HasTrivia = NextHasTrivia;

            _triviaLine = _line;
            _triviaColumn = _column;
            _lastTriviaIndex = _triviaIndex;
            _triviaIndex = _index;

            _tokenLine = _line;
            _tokenColumn = _column;
            _lastTokenIndex = _tokenIndex;
            _tokenIndex = _index;

            ReadToken();
        }

        /// <inheritdoc />
        public abstract bool Check(T flags);

        /// <summary>
        /// Goes to the next token.
        /// </summary>
        protected abstract void ReadToken();

        /// <summary>
        /// Checks whether the current token has flags and is equal to the given string.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="content">The content.</param>
        /// <param name="comparison">The type of comparison for the content.</param>
        /// <returns>
        ///     <c>true</c> if the flags and content match; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Check(T flags, string content, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (Check(flags) && Content.Span.Equals(content.AsSpan(), comparison))
                return true;
            return false;
        }

        /// <inheritdoc />
        public bool Branch(T flag)
        {
            if (Check(flag))
            {
                Next();
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public bool Branch(T flag, string content, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (Check(flag, content, comparison))
            {
                Next();
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public bool Branch(T flag, out Token token)
        {
            if (Check(flag))
            {
                token = Token;
                Next();
                return true;
            }

            token = default;
            return false;
        }

        /// <inheritdoc />
        public bool Branch(T flag, string content, out Token token)
        {
            if (Check(flag, content))
            {
                token = Token;
                Next();
                return true;
            }
            token = default;
            return false;
        }

        /// <inheritdoc />
        public bool Branch(T flag, string content, StringComparison comparison, out Token token)
        {
            if (Check(flag, content, comparison))
            {
                token = Token;
                Next();
                return true;
            }
            token = default;
            return false;
        }

        /// <summary>
        /// Skips contents as long as the token is one of the specified flags.
        /// </summary>
        /// <param name="flags">The flags to check.</param>
        public void Skip(T flags)
        {
            while (Check(flags))
                Next();
        }

        /// <summary>
        /// Marks the current character as a token character.
        /// </summary>
        protected void ContinueToken()
        {
            if (_index < _contents.Length)
            {
                _index++;
                _column++;
            }
        }

        /// <summary>
        /// Marks the current character as trivia. If tokens were
        /// already found previously, the token characters will turn
        /// into trivia.
        /// </summary>
        protected void ContinueTrivia()
        {
            if (_index < _contents.Length)
            {
                _index++;
                _column++;

                _tokenIndex = _index;
                _tokenLine = _line;
                _tokenColumn = _column;
            }
        }

        /// <summary>
        /// Starts a new line (just for line numbering).
        /// </summary>
        protected void Newline()
        {
            _line++;
            _column = 1;
        }

        /// <inheritdoc />
        public Tracker Track()
            => new(_lastTokenIndex, Token.Location);

        /// <inheritdoc />
        public Token GetTracked(Tracker tracker, bool includeCurrentToken = false)
        {
            if (includeCurrentToken)
                return new(tracker.Location, _contents.AsMemory(tracker.Index, _tokenIndex - tracker.Index));
            return new(tracker.Location, _contents.AsMemory(tracker.Index, _lastTokenIndex - tracker.Index));
        }
    }
}