namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// Possible binary operator type.
    /// </summary>
    public enum BinaryOperatortype
    {
        None,
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Modulo,
        ShiftLeft,
        ShiftRight,
        GreaterThan,
        SmallerThan,
        GreaterThanOrEqual,
        SmallerThanOrEqual,
        Equals,
        NotEquals,
        And,
        Or,
        Xor,
        LogicalAnd,
        LogicalOr,
        Concatenate,
        Assignment,
        Backtrack
    }
}
