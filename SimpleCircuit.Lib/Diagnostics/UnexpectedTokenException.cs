using SimpleCircuit.Parser;

namespace SimpleCircuit
{
    /// <summary>
    /// Thrown when a token that was not expected was encountered.
    /// </summary>
    public class UnexpectedTokenException : ParserException
    {
        /// <summary>
        /// Creates a new <see cref="UnexpectedTokenException"/>.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="expected">The expected 'thing'.</param>
        public UnexpectedTokenException(ILexer lexer, string expected)
            : base(lexer, string.Format("Encountered '{0}' while {1} was expected", lexer?.Content, expected))
        {
        }

        /// <summary>
        /// Creates a new <see cref="UnexpectedTokenException"/>.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="format">The expected format.</param>
        /// <param name="args">The arguments.</param>
        public UnexpectedTokenException(ILexer lexer, string format, params object[] args)
            : this(lexer, string.Format(format, args))
        {
        }
    }
}