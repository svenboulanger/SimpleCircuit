namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// Token types for simple text/labels.
    /// </summary>
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
        /// An escaped sequence.
        /// </summary>
        EscapedSequence = 0x04,

        /// <summary>
        /// An escaped character.
        /// </summary>
        EscapedCharacter = 0x08,

        /// <summary>
        /// A subscript.
        /// </summary>
        Subscript = 0x10,

        /// <summary>
        /// A superscript.
        /// </summary>
        Superscript = 0x20,

        /// <summary>
        /// An opening bracket for grouping sub- and superscript.
        /// </summary>
        OpenBracket = 0x40,

        /// <summary>
        /// A closing bracket for grouping sub- and superscript.
        /// </summary>
        CloseBracket = 0x80,

        /// <summary>
        /// All tokens.
        /// </summary>
        All = -1
    }
}
