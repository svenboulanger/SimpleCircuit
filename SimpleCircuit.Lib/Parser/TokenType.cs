using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A token type.
    /// </summary>
    [Flags]
    public enum TokenType
    {
        /// <summary>
        /// End of content.
        /// </summary>
        EndOfContent = 0,

        /// <summary>
        /// Punctuation.
        /// </summary>
        Punctuator = 0x01,

        /// <summary>
        /// A whitespace.
        /// </summary>
        Whitespace = 0x02,

        /// <summary>
        /// A comment.
        /// </summary>
        Comment = 0x04,

        /// <summary>
        /// A word.
        /// </summary>
        Word = 0x08,

        /// <summary>
        /// A number.
        /// </summary>
        Number = 0x010,

        /// <summary>
        /// A quoted string.
        /// </summary>
        String = 0x020,

        /// <summary>
        /// A new line.
        /// </summary>
        Newline = 0x040,

        /// <summary>
        /// An arrow character (unicode).
        /// </summary>
        Arrow = 0x080,

        /// <summary>
        /// An unknown character.
        /// </summary>
        Unknown = 0x0100,

        /// <summary>
        /// All characters
        /// </summary>
        All = -1
    }
}
