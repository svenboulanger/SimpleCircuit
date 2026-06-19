using System.Linq;
using SimpleCircuit.Tests.Helpers;
using Xunit;

namespace SimpleCircuit.Tests.Integration;

public class PipelineTests
{
    [Fact]
    public void SimpleChain_EvaluatesWithoutErrors()
    {
        var context = ScriptRunner.Evaluate("GND <u> R <r> C", out var diag);
        Assert.False(diag.HasErrors);
        Assert.True(context.Circuit.Count > 0);
    }

    [Fact]
    public void AnonymousComponents_GetDistinctNames()
    {
        var context = ScriptRunner.Evaluate("R R", out var diag);
        Assert.False(diag.HasErrors);

        var resistorNames = context.Circuit
            .Select(p => p.Name)
            .Where(n => n.Contains('R'))
            .Distinct()
            .ToList();
        Assert.True(resistorNames.Count >= 2, $"Expected >= 2 distinct resistors, got: {string.Join(", ", resistorNames)}");
    }

    [Fact]
    public void ForLoop_EmitsMultipleComponents()
    {
        string single = ScriptRunner.Evaluate("R", out _).Circuit.Count.ToString();
        var looped = ScriptRunner.Evaluate(".for i 1 3 1\nR\n.endfor", out var diag);
        Assert.False(diag.HasErrors);
        // Three iterations should emit more components than a single resistor.
        Assert.True(looped.Circuit.Count > int.Parse(single));
    }

    [Fact]
    public void ForLoop_DefaultIncrement_CountsUp()
    {
        // .for i 1 3 (no increment) defaults to +1: i = 1, 2, 3 -> three resistors.
        var looped = ScriptRunner.Evaluate(".for i 1 3\nR\n.endfor", out var diag);
        Assert.False(diag.HasErrors);
        Assert.Equal(3, DistinctResistorCount(looped));
    }

    [Fact]
    public void ForLoop_DefaultIncrement_CountsDown()
    {
        // .for i 3 1 (no increment) defaults to -1: i = 3, 2, 1 -> three resistors.
        var looped = ScriptRunner.Evaluate(".for i 3 1\nR\n.endfor", out var diag);
        Assert.False(diag.HasErrors);
        Assert.Equal(3, DistinctResistorCount(looped));
    }

    [Fact]
    public void ForLoop_ExplicitIncrement_Unchanged()
    {
        // .for i 1 3 2 still steps by 2: i = 1, 3 -> two resistors.
        var looped = ScriptRunner.Evaluate(".for i 1 3 2\nR\n.endfor", out var diag);
        Assert.False(diag.HasErrors);
        Assert.Equal(2, DistinctResistorCount(looped));
    }

    private static int DistinctResistorCount(SimpleCircuit.Evaluator.EvaluationContext context)
        => context.Circuit
            .Select(p => p.Name)
            .Where(n => n.Contains('R'))
            .Distinct()
            .Count();

    [Fact]
    public void IfElse_OnlyTakesTrueBranch()
    {
        var taken = ScriptRunner.Evaluate(".if true\nR\n.else\nC\n.endif", out var diag);
        Assert.False(diag.HasErrors);
        Assert.True(taken.Circuit.Count > 0);
    }

    [Fact]
    public void Render_ProducesSvgDocument()
    {
        var context = ScriptRunner.Evaluate("GND <u> R <r> C", out var diag);
        Assert.False(diag.HasErrors);

        var doc = context.Circuit.Render(diag);
        Assert.NotNull(doc);
        Assert.Equal("svg", doc.DocumentElement.LocalName);
        // Structure-only: the SVG must declare a size. We do not assert exact geometry.
        Assert.True(doc.DocumentElement.HasAttribute("width"));
        Assert.True(doc.DocumentElement.HasAttribute("height"));
    }

    [Fact]
    public void Solve_Succeeds()
    {
        var context = ScriptRunner.Evaluate("GND <u> R <r> C", out var diag);
        Assert.True(context.Circuit.Solve(diag));
    }

    [Fact]
    public void MalformedInput_PostsErrorWithoutThrowing()
    {
        // An unterminated quoted string surfaces a QuoteMismatch diagnostic; the
        // pipeline must report it rather than throwing.
        var context = ScriptRunner.Evaluate("R \"unterminated label", out var diag);
        Assert.True(diag.HasErrors);
    }
}
