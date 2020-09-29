namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A token type.
    /// </summary>
    public enum TokenType
    {
        Dot,
        Dash,
        Plus,
        Times,
        Divide,
        Whitespace,
        Word,
        Number,
        String,
        Newline,
        OpenBracket,
        CloseBracket,
        Equals,
        EndOfContent
    }
}
