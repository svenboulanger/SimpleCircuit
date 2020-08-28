using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// An exception for a lexer.
    /// </summary>
    /// <seealso cref="Exception" />
    public class LexerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LexerException(string message)
            : base(message)
        {
        }
    }
}
