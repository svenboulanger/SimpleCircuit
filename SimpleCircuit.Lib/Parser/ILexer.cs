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
        /// Gets the current line.
        /// </summary>
        public ReadOnlyMemory<char> CurrentLine { get; }

        /// <summary>
        /// Gets the last parsed line.
        /// </summary>
        public ReadOnlyMemory<char> LastLine { get; }

        /// <summary>
        /// Gets the current token's content.
        /// </summary>
        public ReadOnlyMemory<char> Content { get; }

        /// <summary>
        /// Gets the current token's trivia.
        /// </summary>
        public ReadOnlyMemory<char> Trivia { get; }

        /// <summary>
        /// Gets the trivia before this token's content.
        /// </summary>
        public ReadOnlyMemory<char> ContentWithTrivia { get; }

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public Token Token { get; }

        /// <summary>
        /// Gets a token representing the start of the current token.
        /// </summary>
        public Token StartToken { get; }

        /// <summary>
        /// Gets the current token's line number.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the current token's column index.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the length of the token.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Determines whether the current token has trivia leading up to it.
        /// </summary>
        bool HasTrivia { get; }
    }
}