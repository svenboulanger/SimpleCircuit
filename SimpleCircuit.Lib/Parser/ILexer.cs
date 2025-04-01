using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Describes a lexer.
    /// </summary>
    public interface ILexer
    {
        /// <summary>
        /// Gets the source of the lexer.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public Token Token { get; }

        /// <summary>
        /// Gets the next token.
        /// </summary>
        public Token NextToken { get; }

        /// <summary>
        /// Goes to the next token.
        /// </summary>
        public void Next();
    }

    /// <summary>
    /// Describes a lexer.
    /// </summary>
    public interface ILexer<T>
    {
        /// <summary>
        /// Gets the next token type.
        /// </summary>
        public T NextType { get; }

        /// <summary>
        /// Gets the current token type.
        /// </summary>
        public T Type { get; }

        /// <summary>
        /// Checks whether the current token has flags.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if there are flags; otherwise, <c>false</c>.
        /// </returns>
        public bool Check(T flags);

        /// <summary>
        /// Checks whether the current token matches the flag and consumes it if it matches.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <returns>
        ///     <c>true</c> if the token matched and was consumed; otherwise, <c>false</c>.
        /// </returns>
        public bool Branch(T flag);

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
        public bool Branch(T flag, string content, StringComparison comparison = StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks whether the current token matches the flag, and consumes it if it matches.
        /// The contents of the matched token is returned.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="token">The matched token.</param>
        /// <returns>
        ///     <c>true</c> if the current token matched and was consumed; otherwise, <c>false</c>.
        /// </returns>
        public bool Branch(T flag, out Token token);

        /// <summary>
        /// Checks whether current token matches the flag and content and consumes it
        /// if it matches.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="content">The content to match.</param>
        /// <param name="token">The matched token.</param>
        /// <returns>
        ///     <c>true</c> if the current token matched and was consumed; otherwise, <c>false</c>.
        /// </returns>
        public bool Branch(T flag, string content, out Token token);

        /// <summary>
        /// Checks whether current token matches the flag and content and consumes it
        /// if it matches.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="content">The content to match.</param>
        /// <param name="comparison">The type of comparison for the content.</param>
        /// <param name="token">The matched token.</param>
        /// <returns>
        ///     <c>true</c> if the current token matched and was consumed; otherwise, <c>false</c>.
        /// </returns>
        public bool Branch(T flag, string content, StringComparison comparison, out Token token);

        /// <summary>
        /// Creates a tracker that allows to track combined tokens using <see cref="GetTracked(Tracker, bool)"/>.
        /// </summary>
        /// <param name="includeTrivia">If <c>true</c>, the trivia is included in the tracker.</param>
        /// <returns>Returns the tracker.</returns>
        public Tracker Track(bool includeTrivia = false);

        /// <summary>
        /// Gets a token that contains the combined content since the tracker.
        /// </summary>
        /// <param name="tracker">The tracker.</param>
        /// <param name="includeCurrentToken">If <c>true</c>, the current token is included in the tracked token.</param>
        /// <returns>The tracked token.</returns>
        public Token GetTracked(Tracker tracker, bool includeCurrentToken = false);
    }
}