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
    }
}
