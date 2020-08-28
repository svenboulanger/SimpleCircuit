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
        Whitespace,
        Word,
        Number,
        Newline,
        OpenBracket,
        CloseBracket,
        Equals,
        EndOfContent
    }
}
