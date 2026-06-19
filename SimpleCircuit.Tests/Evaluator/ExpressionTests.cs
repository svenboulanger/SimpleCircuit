using SimpleCircuit.Tests.Helpers;
using Xunit;

namespace SimpleCircuit.Tests.Evaluator;

public class ExpressionTests
{
    // Note: the operators '%' (modulo) and '^' (xor) exist in the expression parser
    // and evaluator, but are not produced by the lexer, so they are not usable from
    // a script and are intentionally not tested here.
    [Theory]
    [InlineData("1+2", 3.0)]
    [InlineData("2*3+4", 10.0)]   // precedence
    [InlineData("2+3*4", 14.0)]
    [InlineData("(2+3)*4", 20.0)] // brackets
    [InlineData("10/4", 2.5)]
    [InlineData("-5", -5.0)]
    [InlineData("+5", 5.0)]
    public void Arithmetic(string expr, double expected)
        => Assert.Equal(expected, (double)ExpressionRunner.Evaluate(expr), 9);

    [Theory]
    [InlineData("1k", 1000.0)]
    [InlineData("2.2k", 2200.0)]
    [InlineData("1K", 1000.0)]    // case insensitive
    [InlineData("1m", 1e-3)]
    [InlineData("1meg", 1e6)]
    [InlineData("1u", 1e-6)]
    [InlineData("1n", 1e-9)]
    [InlineData("1p", 1e-12)]
    [InlineData("1f", 1e-15)]
    [InlineData("1G", 1e9)]
    [InlineData("1T", 1e12)]
    public void SiSuffixes(string expr, double expected)
        => Assert.Equal(expected, (double)ExpressionRunner.Evaluate(expr), 9);

    // Regression: pure scientific-notation numbers (e.g. "1e3", "1.5e-3") used to
    // overflow the index in ExpressionParser.ParseNumber and throw
    // ArgumentOutOfRangeException. Fixed by ending the exponent branch with an explicit
    // break so the for-loop's i++ no longer overshoots the string length.
    [Theory]
    [InlineData("1e3", 1000.0)]
    [InlineData("1.5e-3", 1.5e-3)]
    public void ScientificNotation(string expr, double expected)
        => Assert.Equal(expected, (double)ExpressionRunner.Evaluate(expr), 9);

    [Theory]
    [InlineData("1 < 2", true)]
    [InlineData("2 <= 2", true)]
    [InlineData("3 == 3", true)]
    [InlineData("3 != 4", true)]
    [InlineData("2 > 5", false)]
    [InlineData("2 >= 5", false)]
    [InlineData("true && false", false)]
    [InlineData("true || false", true)]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    public void BooleanLogic(string expr, bool expected)
        => Assert.Equal(expected, (bool)ExpressionRunner.Evaluate(expr));

    [Theory]
    [InlineData("6 & 3", 2.0)]
    [InlineData("4 | 1", 5.0)]
    [InlineData("1 << 3", 8.0)]
    [InlineData("16 >> 2", 4.0)]
    public void Bitwise(string expr, double expected)
        => Assert.Equal(expected, (double)ExpressionRunner.Evaluate(expr), 9);

    [Theory]
    [InlineData("1 < 2 ? 10 : 20", 10.0)]
    [InlineData("1 > 2 ? 10 : 20", 20.0)]
    [InlineData("0 ? 10 : 20", 20.0)]   // numeric zero is falsy
    [InlineData("5 ? 10 : 20", 10.0)]   // numeric non-zero is truthy
    public void Ternary(string expr, double expected)
        => Assert.Equal(expected, (double)ExpressionRunner.Evaluate(expr), 9);

    [Theory]
    [InlineData("\"a\" + \"b\"", "ab")]
    [InlineData("\"x\" + 1", "x1")]
    [InlineData("1 + \"x\"", "1x")]
    public void StringConcatenation(string expr, string expected)
        => Assert.Equal(expected, (string)ExpressionRunner.Evaluate(expr));

    [Fact]
    public void Vector_TwoComponents_BuildsVector2()
        => Assert.Equal(new Vector2(1, 2), (Vector2)ExpressionRunner.Evaluate("1, 2"));

    [Theory]
    [InlineData("round(2.4)", 2.0)]
    [InlineData("round(2.5)", 3.0)]      // round half away from zero
    [InlineData("round(3.5)", 4.0)]
    [InlineData("round(-2.5)", -3.0)]
    [InlineData("round(2.345, 2)", 2.35)]
    public void RoundFunction(string expr, double expected)
        => Assert.Equal(expected, (double)ExpressionRunner.Evaluate(expr), 9);

    [Fact]
    public void UnknownFunction_PostsDiagnostic()
    {
        var result = ExpressionRunner.Evaluate("foo(1)", out var diag);
        Assert.Null(result);
        Assert.True(diag.HasErrors);
    }

    // Regression: the binary "CouldNotOperateForArgumenttype" message has three
    // placeholders ({0} operation, {1}/{2} types) but ExpressionEvaluator.Evaluate(BinaryNode)
    // used to post only two arguments, so string.Format threw a FormatException instead of
    // reporting a clean diagnostic. Fixed by also passing the operation name.
    [Fact]
    public void TypeMismatch_PostsDiagnostic()
    {
        ExpressionRunner.Evaluate("\"a\" - 1", out var diag);
        Assert.True(diag.HasErrors);
    }

    [Fact]
    public void UnknownVariable_PostsDiagnostic()
    {
        var result = ExpressionRunner.Evaluate("undefinedVariable", out var diag);
        Assert.Null(result);
        Assert.True(diag.HasErrors);
    }
}
