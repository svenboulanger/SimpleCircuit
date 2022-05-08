using SimpleCircuit.Parser;
using System;

namespace SimpleCircuit
{
    /// <summary>
    /// An exception that can be used to indicate parser errors.
    /// </summary>
    public class ParserException : Exception
    {
        /// <summary>
        /// Gets the line.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the column.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the content of the token where it failed.
        /// </summary>
        public ReadOnlyMemory<char> Content { get; }

        /// <summary>
        /// Gets the inner message.
        /// </summary>
        public string InnerMessage { get; }

        /// <summary>
        /// Creates a new parser exception.
        /// </summary>
        /// <param name="message">The message.</param>
        public ParserException(string message)
            : base(message)
        {
            InnerMessage = message;
        }

        /// <summary>
        /// Creates a new parser exception.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="message">The message.</param>
        public ParserException(ILexer lexer, string message)
            : base($"{message} at line {lexer?.Line ?? 0}, {lexer?.Column ?? 0}.")
        {
            InnerMessage = message;
            if (lexer != null)
            {
                Line = lexer.Line;
                Column = lexer.Column;
                Content = lexer.Token;
            }
        }

        /// <summary>
        /// Creates a new parser exception.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        /// <param name="content">The content.</param>
        /// <param name="message">The message.</param>
        public ParserException(int line, int column, ReadOnlyMemory<char> content, string message)
            : base($"{message} at line {line}, {column}.")
        {
            InnerMessage = message;
            Line = line;
            Column = column;
            Content = content;
        }

        /// <summary>
        /// Creates a new parser exception.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="message">The message.</param>
        public ParserException(ILexer lexer, string format, params object[] args)
            : this(lexer, string.Format(format, args))
        {
            InnerMessage = string.Format(format, args);
        }
    }
}