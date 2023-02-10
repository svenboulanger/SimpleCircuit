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
        /// Determines whether the current token has trivia leading up to it.
        /// </summary>
        bool HasTrivia { get; }
    }
}