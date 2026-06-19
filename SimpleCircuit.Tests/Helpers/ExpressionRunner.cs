using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Evaluator;
using SimpleCircuit.Parser;

namespace SimpleCircuit.Tests.Helpers;

/// <summary>
/// Helper to lex, parse and evaluate a single SimpleCircuit expression string.
/// </summary>
public static class ExpressionRunner
{
    /// <summary>
    /// Lexes, parses and evaluates an expression, returning the resulting value
    /// (or <c>null</c>) and the diagnostics that were posted.
    /// </summary>
    public static object Evaluate(string expression, out TestDiagnosticHandler diagnostics)
    {
        diagnostics = new TestDiagnosticHandler();
        var lexer = SimpleCircuitLexer.FromString(expression, "test");
        lexer.Diagnostics = diagnostics;
        var context = new ParsingContext { Diagnostics = diagnostics };
        if (!ExpressionParser.ParseExpression(lexer, context, out var node) || node is null)
            return null;

        // No assembly loading needed: pure expressions do not require component factories.
        var evalContext = new EvaluationContext(false, new Style(), null) { Diagnostics = diagnostics };
        return ExpressionEvaluator.Evaluate(node, evalContext);
    }

    /// <summary>
    /// Convenience overload that discards the diagnostics.
    /// </summary>
    public static object Evaluate(string expression) => Evaluate(expression, out _);
}
