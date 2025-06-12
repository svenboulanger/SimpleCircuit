namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// Possible binary operator types.
    /// </summary>
    public enum BinaryOperatorTypes
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
    }
}
