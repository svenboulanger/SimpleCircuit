using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// An exception for a lexer.
    /// </summary>
    /// <seealso cref="Exception" />
    public class ParseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ParseException(string message, int line, int position)
            : base($"{message} at line {line}, position {position}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="message">The message.</param>
        public ParseException(Lexer lexer, string message)
            : base($"{message} at line {lexer.Line}, column {lexer.Column}")
        {
        }
    }
}
