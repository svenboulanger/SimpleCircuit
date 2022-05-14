using System;
using System.IO;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A lexer.
    /// </summary>
    /// <typeparam name="T">The token type.</typeparam>
    public abstract class Lexer<T> : ILexer
    {
        private readonly string _contents;
        private int _lastLineIndex = 0, _line, _trivia = 0;

        /// <inheritdoc/>
        public ReadOnlyMemory<char> CurrentLine => _contents.AsMemory(_lastLineIndex, Index - _lastLineIndex);

        /// <inheritdoc/>
        public ReadOnlyMemory<char> LastLine { get; private set; }

        /// <inheritdoc/>
        public ReadOnlyMemory<char> Token => _contents.AsMemory(Index - Length, Length);

        /// <inheritdoc/>
        public ReadOnlyMemory<char> Trivia => _contents.AsMemory(Index - Length - _trivia, _trivia);

        /// <inheritdoc/>
        public ReadOnlyMemory<char> TokenWithTrivia => _contents.AsMemory(Index - Length - _trivia, Length + _trivia);

        /// <summary>
        /// Gets the current token type.
        /// </summary>
        public T Type { get; protected set; }

        /// <inheritdoc/>
        public int Line
        {
            get
            {
                int index = Index - _lastLineIndex - Length + 1;
                if (index <= 0)
                    return _line - 1;
                return _line;
            }
        }

        /// <inheritdoc/>
        public int Column
        {
            get
            {
                int index = Index - _lastLineIndex - Length + 1;
                if (index <= 0)
                    return LastLine.Length + index;
                return index;
            }

        }

        /// <summary>
        /// Gets the current index.
        /// </summary>
        public int Index { get; private set; }

        /// <inheritdoc/>
        public int Length { get; private set; }

        /// <inheritdoc/>
        public bool HasTrivia => _trivia > 0;

        /// <summary>
        /// Gets the current character.
        /// </summary>
        protected char Char
        {
            get
            {
                if (Index >= _contents.Length)
                    return '\0';
                return _contents[Index];
            }
        }

        /// <summary>
        /// Creates a new <see cref="Lexer{T}"/>.
        /// </summary>
        /// <param name="reader">The text reader.</param>
        /// <param name="line">The initial line number.</param>
        protected Lexer(TextReader reader, int line = 1)
        {
            using (reader)
            {
                _contents = reader.ReadToEnd();
            }
            _line = line;
            Next();
        }


        /// <summary>
        /// Creates a new <see cref="Lexer{T}"/>.
        /// </summary>
        /// <param name="netlist">The source.</param>
        /// <param name="line">The initial line number.</param>
        protected Lexer(string netlist, int line = 1)
        {
            if (string.IsNullOrEmpty(netlist))
                _contents = "";
            else
                _contents = netlist;
            _line = line;
            Next();
        }

        /// <summary>
        /// Goes to the next token.
        /// </summary>
        public void Next()
        {
            Length = 0;
            _trivia = 0;
            ReadToken();
        }

        /// <summary>
        /// Checks whether the current token has flags.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if there are flags; otherwise, <c>false</c>.
        /// </returns>
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
            if (Check(flags) && Token.Span.Equals(content.AsSpan(), comparison))
                return true;
            return false;
        }

        /// <summary>
        /// Checks whether the current token matches the flag and consumes it if it matches.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <returns>
        ///     <c>true</c> if the token matched and was consumed; otherwise, <c>false</c>.
        /// </returns>
        public bool Branch(T flag)
        {
            if (Check(flag))
            {
                Next();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether current token matches the flag and a first character and consumes it
        /// if it matches.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="first">The first character of the content.</param>
        /// <returns>
        ///     <c>true</c> if the current token matched and was consumed; otherwise, <c>false</c>.
        /// </returns>
        public bool Branch(T flag, char first)
        {
            if (Check(flag) && Token.Span[0] == first)
            {
                Next();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether current token matches the flag and content and consumes it
        /// if it matches.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="content">The content to match.</param>
        /// <param name="comparison">The type of comparison for the content.</param>
        /// <returns>
        ///     <c>true</c> if the current token matched and was consumed; otherwise, <c>false</c>.
        /// </returns>
        public bool Branch(T flag, string content, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (Check(flag, content, comparison))
            {
                Next();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the current token matches the flag, and consumes it if it matches.
        /// The contents of the matched token is returned.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="content">The content of the matched token.</param>
        /// <returns>
        ///     <c>true</c> if the current token matched and was consumed; otherwise, <c>false</c>.
        /// </returns>
        public bool Branch(T flag, out ReadOnlyMemory<char> content)
        {
            if (Check(flag))
            {
                content = Token;
                Next();
                return true;
            }

            content = new();
            return false;
        }

        /// <summary>
        /// Checks whether the current token matches the flag, and consumes the token if it matches.
        /// If not, an exception is thrown.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="shouldNotContainTrivia">If <c>false</c>, the method also throws an exception if the token contains trivia.</param>
        /// <returns>The lexer itself, which can be used for chaining together multiple methods.</returns>
        /// <exception cref="UnexpectedTokenException">Thrown if the current token does not match the flag.</exception>
        public Lexer<T> Expect(T flag, bool shouldNotContainTrivia = false)
        {
            if (shouldNotContainTrivia && HasTrivia)
                throw new UnexpectedTokenException(this, flag.ToString());
            if (Branch(flag))
                return this;
            throw new UnexpectedTokenException(this, flag.ToString());
        }

        /// <summary>
        /// Checks whether the current token matches the flag and first character, and consumes the token if
        /// it matches. If not, an exception is thrown.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="first">The first character.</param>
        /// <param name="shouldNotContainTrivia">If <c>false</c>, the method also throws an exception if the token contains trivia.</param>
        /// <returns>The lexer itself, which can be used for chaining together multiple methods.</returns>
        /// <exception cref="UnexpectedTokenException">Thrown if the current token does not match the flag or first character.</exception>
        public Lexer<T> Expect(T flag, char first, bool shouldNotContainTrivia = false)
        {
            if (shouldNotContainTrivia && HasTrivia)
                throw new UnexpectedTokenException(this, first.ToString());
            if (Branch(flag, first))
                return this;
            throw new UnexpectedTokenException(this, first.ToString());
        }

        /// <summary>
        /// Checks whether the current token matches the flag and content, and consumes the token if it matches.
        /// If not, an exception is thrown.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="content">The content.</param>
        /// <param name="shouldNotContainTrivia">If <c>false</c>, the method also throws an exception if the token contains trivia.</param>
        /// <param name="comparison">The type of comparison for the content.</param>
        /// <returns>The lexer itself, which can be used for chaining together multiple methods.</returns>
        /// <exception cref="UnexpectedTokenException">Thrown if the current token does not match the flag and content.</exception>
        public Lexer<T> Expect(T flag, string content, bool shouldNotContainTrivia = false, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (shouldNotContainTrivia && HasTrivia)
                throw new UnexpectedTokenException(this, content);
            if (Branch(flag, content, comparison))
                return this;
            throw new UnexpectedTokenException(this, content);
        }

        /// <summary>
        /// Checks whether the current token matches the flag, and consumes the token if it matches.
        /// If not, an exception is thrown.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="content">The content of the matched token.</param>
        /// <param name="shouldNotContainTrivia">If <c>false</c>, the method also throws an exception if the token contains trivia.</param>
        /// <exception cref="UnexpectedTokenException">Thrown if the current token does not match the flag.</exception>
        public Lexer<T> Expect(T flag, out ReadOnlyMemory<char> content, bool shouldNotContainTrivia = false)
        {
            if (shouldNotContainTrivia && HasTrivia)
                throw new UnexpectedTokenException(this, flag.ToString());
            if (Check(flag))
            {
                content = Token;
                Next();
                return this;
            }
            throw new UnexpectedTokenException(this, flag.ToString());
        }

        /// <summary>
        /// Reads contents as long as the token is one of the specified flags.
        /// </summary>
        /// <param name="flags">The flags to check.</param>
        /// <param name="shouldNotIncludeTrivia">If <c>false</c>, trivia is read along with the tokens.</param>
        /// <returns>The contents.</returns>
        public ReadOnlyMemory<char> ReadWhile(T flags, bool shouldNotIncludeTrivia = false)
        {
            // We include the current token
            int start = Index - Length;

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
                result = _contents.AsMemory(start, Index - _trivia - Length - start);
            }
            else
            {
                // There is nothing here!
                result = new();
            }

            // The current token should not be considered
            return result;
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
            if (Index < _contents.Length)
            {
                Index++;
                Length++;
            }
        }

        /// <summary>
        /// Marks the current character as trivia. If tokens were
        /// already found previously, the token characters will turn
        /// into trivia.
        /// </summary>
        protected void ContinueTrivia()
        {
            if (Index < _contents.Length)
            {
                Index++;
                _trivia += Length + 1;
                Length = 0;
            }
        }

        /// <summary>
        /// Stores the last line.
        /// </summary>
        protected void StoreLine()
        {
            LastLine = _contents.AsMemory(_lastLineIndex, Index - _lastLineIndex);
        }

        /// <summary>
        /// Starts a new line (just for line numbering).
        /// </summary>
        protected void Newline()
        {
            StoreLine();
            _lastLineIndex = Index;
            _line++;
        }

        /// <summary>
        /// Begins tracking any parsed input from the lexer. Tracking can be nested.
        /// </summary>
        /// <param name="includeTrivia">If <c>true</c>, the trivia leading up to this token are included.</param>
        /// <returns>Returns the index at the start of the current token.</returns>
        public int TrackIndex(bool includeTrivia = false)
        {
            if (includeTrivia)
                return Index - Length - _trivia;
            else
                return Index - Length;
        }

        /// <summary>
        /// Stops tracking any parsed input from the lexer and returns the result. Tracking can be nested.
        /// </summary>
        /// <param name="includeCurrentToken">If <c>true</c>, the current token is included.</param>
        /// <returns>The tracked input.</returns>
        public ReadOnlyMemory<char> Track(int start, bool includeCurrentToken = false)
        {
            if (includeCurrentToken)
                return _contents.AsMemory(start, Index - start);
            return _contents.AsMemory(start, Index - Length - _trivia - start);
        }
    }
}