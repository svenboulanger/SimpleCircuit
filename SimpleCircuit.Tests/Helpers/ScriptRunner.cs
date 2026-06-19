using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Evaluator;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;

namespace SimpleCircuit.Tests.Helpers;

/// <summary>
/// Helpers to drive the SimpleCircuit pipeline (lexer → parser → evaluator) from a
/// script string, modeled on <c>Job.Compute()</c>.
/// </summary>
public static class ScriptRunner
{
    /// <summary>
    /// Parses a script into its AST. Returns the parser success flag and the
    /// resulting statements, and exposes the diagnostics that were posted.
    /// </summary>
    public static bool TryParse(string script, out ScopedStatementsNode statements, out TestDiagnosticHandler diagnostics)
    {
        diagnostics = new TestDiagnosticHandler();
        var lexer = SimpleCircuitLexer.FromString(script, "test");
        lexer.Diagnostics = diagnostics;
        var context = new ParsingContext { Diagnostics = diagnostics };
        return SimpleCircuitParser.Parse(lexer, context, out statements);
    }

    /// <summary>
    /// Runs a full parse + evaluate of a script and returns the evaluation context
    /// together with the diagnostics that were posted along the way.
    /// </summary>
    public static EvaluationContext Evaluate(string script, out TestDiagnosticHandler diagnostics)
    {
        diagnostics = new TestDiagnosticHandler();
        var lexer = SimpleCircuitLexer.FromString(script, "test");
        lexer.Diagnostics = diagnostics;

        var parsingContext = new ParsingContext { Diagnostics = diagnostics };
        var context = new EvaluationContext(true, new Style(), null) { Diagnostics = diagnostics };
        if (SimpleCircuitParser.Parse(lexer, parsingContext, out var statements))
            StatementEvaluator.Evaluate(statements, context);
        return context;
    }
}
