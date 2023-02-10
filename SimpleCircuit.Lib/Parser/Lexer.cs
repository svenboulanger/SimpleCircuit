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
        private readonly ReadOnlyMemory<char> _contents;
        private int _line, _trivia = 0, _index = 0, _length = 0, _column = 1;
        private TextLocation _triviaStart, _tokenStart;
        private bool _isTrivia = true;

        /// <inheritdoc />
        public string Source { get; }

        /// <inheritdoc/>
        public ReadOnlyMemory<char> Content => _contents[(_index-_length).._index];

        /// <inheritdoc />
        public Token Token => new(Source, _tokenStart, Content);

        /// <summary>
        /// Gets the current token type.
        /// </summary>
        public T Type { get; protected set; }

        /// <inheritdoc/>
        public bool HasTrivia => _trivia > 0;

        /// <summary>
        /// Gets the current character.
        /// </summary>
        protected char Char
        {
            get
            {
                if (_index >= _contents.Length)
                    return '\0';
                return _contents.Span[_index];
            }
        }

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
                _contents = reader.ReadToEnd().AsMemory();
            }
            _line = line;
            Next();
        }


        /// <summary>
        /// Creates a new <see cref="Lexer{T}"/>.
        /// </summary>
        /// <param name="netlist">The source.</param>
        /// <param name="source">The source.</param>
        /// <param name="line">The initial line number.</param>
        protected Lexer(ReadOnlyMemory<char> netlist, string source = null, int line = 1)
        {
            Source = source;
            _contents = netlist;
            _line = line;
            Next();
        }

        /// <inheritdoc />
        public void Next()
        {
            _length = 0;
            _trivia = 0;
            _triviaStart = new(_line, _column);
            _isTrivia = true;
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

        /// <summary>
        /// Reads contents as long as the token is one of the specified flags.
        /// </summary>
        /// <param name="flags">The flags to check.</param>
        /// <param name="shouldNotIncludeTrivia">If <c>false</c>, trivia is read along with the tokens.</param>
        /// <returns>The contents.</returns>
        public Token ReadWhile(T flags, bool shouldNotIncludeTrivia = false)
        {
            // We include the current token
            int startIndex = _index - _length;
            var startToken = _tokenStart;

            // Consume the first one
            ReadOnlyMemory<char> result;
            if (Check(flags))
            {
                Next();

                // Keep reading as long as we don't have any trivia and the flag matches
                while (Check(flags))
                {
                    if (shouldNotIncludeTrivia && HasTrivia)
                        break;
                    Next();
                }

                // Take the relevant memory chunk
                result = _contents[startIndex..(_index - _trivia - _length)];
            }
            else
            {
                // There is nothing here!
                result = new();
            }

            // The current token should not be considered
            return new(Source, startToken, result);
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
                if (_isTrivia)
                {
                    _isTrivia = false;
                    _tokenStart = new(_line, _column);
                }
                _column++;
                _index++;
                _length++;
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
                _isTrivia = true;
                _index++;
                _trivia += _length + 1;
                _length = 0;
            }
        }

        /// <summary>
        /// Starts a new line (just for line numbering).
        /// </summary>
        protected void Newline() => _line++;

        /// <inheritdoc />
        public Tracker Track(bool includeTrivia = false)
        {
            if (includeTrivia)
                return new(_index - _length - _trivia, _triviaStart);
            return new(_index - _length, _tokenStart);
        }

        /// <inheritdoc />
        public Token GetTracked(Tracker tracker, bool includeCurrentToken = false)
        {
            if (includeCurrentToken)
                return new(Source, tracker.Location, _contents[tracker.Index.._index]);
            return new(Source, tracker.Location, _contents[tracker.Index..(_index - _length - _trivia)]);
        }
    }
}