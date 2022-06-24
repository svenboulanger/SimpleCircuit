namespace SimpleCircuit.Parser.SimpleTexts
{
    public enum TokenType
    {
        /// <summary>
        /// End of the content.
        /// </summary>
        EndOfContent = 0,

        /// <summary>
        /// Newline identifier.
        /// </summary>
        Newline = 0x01,

        /// <summary>
        /// A character.
        /// </summary>
        Character = 0x02,

        /// <summary>
        /// An escaped character.
        /// </summary>
        Escaped = 0x04,

        /// <summary>
        /// A subscript.
        /// </summary>
        Subscript = 0x08,

        /// <summary>
        /// A superscript.
        /// </summary>
        Superscript = 0x10,

        /// <summary>
        /// An opening bracket for grouping sub- and superscript.
        /// </summary>
        OpenBracket = 0x20,

        /// <summary>
        /// A closing bracket for grouping sub- and superscript.
        /// </summary>
        CloseBracket = 0x40,

        /// <summary>
        /// All tokens.
        /// </summary>
        All = -1
    }
}
