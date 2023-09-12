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
        /// A dot.
        /// </summary>
        Dot = 0x01,

        /// <summary>
        /// A dash.
        /// </summary>
        Dash = 0x02,

        /// <summary>
        /// A plus sign.
        /// </summary>
        Plus = 0x04,

        /// <summary>
        /// An asterisk.
        /// </summary>
        Times = 0x08,

        /// <summary>
        /// A slash.
        /// </summary>
        Divide = 0x10,

        /// <summary>
        /// A whitespace.
        /// </summary>
        Whitespace = 0x20,

        /// <summary>
        /// A comment.
        /// </summary>
        Comment = 0x40,

        /// <summary>
        /// A word.
        /// </summary>
        Word = 0x80,

        /// <summary>
        /// A number.
        /// </summary>
        Number = 0x100,

        /// <summary>
        /// A quoted string.
        /// </summary>
        String = 0x200,

        /// <summary>
        /// A new line.
        /// </summary>
        Newline = 0x400,

        /// <summary>
        /// An opening square bracket.
        /// </summary>
        OpenIndex = 0x800,

        /// <summary>
        /// An opening parenthesis.
        /// </summary>
        OpenParenthesis = 0x1000,

        /// <summary>
        /// An opening beak.
        /// </summary>
        OpenBeak = 0x2000,

        /// <summary>
        /// A closing square bracket.
        /// </summary>
        CloseIndex = 0x4000,

        /// <summary>
        /// A closing parenthesis.
        /// </summary>
        CloseParenthesis = 0x8000,

        /// <summary>
        /// A closing beak.
        /// </summary>
        CloseBeak = 0x10000,

        /// <summary>
        /// An equality sign.
        /// </summary>
        Equals = 0x20000,

        /// <summary>
        /// A question.
        /// </summary>
        Question = 0x40000,

        /// <summary>
        /// A comma.
        /// </summary>
        Comma = 0x80000,

        /// <summary>
        /// An integer.
        /// </summary>
        Integer = 0x100000,

        /// <summary>
        /// An arrow character (unicode).
        /// </summary>
        Arrow = 0x200000,

        /// <summary>
        /// A pipe character.
        /// </summary>
        Pipe = 0x400000,

        /// <summary>
        /// A double pipe sequence.
        /// </summary>
        DoublePipe = 0x800000,

        /// <summary>
        /// An unknown character.
        /// </summary>
        Unknown = 0x1000000,

        /// <summary>
        /// All characters
        /// </summary>
        All = -1
    }
}
