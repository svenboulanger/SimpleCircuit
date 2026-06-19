using System.Linq;
using SimpleCircuit.Parser.Nodes;
using SimpleCircuit.Tests.Helpers;
using Xunit;

namespace SimpleCircuit.Tests.Parser;

public class SimpleCircuitParserTests
{
    [Fact]
    public void SingleComponent_ParsesToComponentChain()
    {
        bool ok = ScriptRunner.TryParse("R", out var statements, out var diag);
        Assert.True(ok);
        Assert.False(diag.HasErrors);
        Assert.Single(statements.Statements);
        Assert.IsType<ComponentChain>(statements.Statements[0]);
    }

    [Fact]
    public void Chain_ParsesAllItems()
    {
        ScriptRunner.TryParse("GND <u> R <r> C", out var statements, out var diag);
        Assert.False(diag.HasErrors);
        var chain = Assert.IsType<ComponentChain>(statements.Statements[0]);
        Assert.True(chain.Items.Length >= 5); // GND, wire, R, wire, C
    }

    [Fact]
    public void VirtualChain_ParsesToVirtualChainNode()
    {
        ScriptRunner.TryParse("(x R1 R2)", out var statements, out var diag);
        Assert.False(diag.HasErrors);
        Assert.Contains(statements.Statements, s => s is VirtualChainNode);
    }

    [Fact]
    public void ParameterDefinition_ParsesNameAndValue()
    {
        ScriptRunner.TryParse(".param width = 5", out var statements, out var diag);
        Assert.False(diag.HasErrors);

        // A .param can be collected either as a statement or as a parameter definition.
        var param = statements.ParameterDefinitions
            .Concat(statements.Statements.OfType<ParameterDefinitionNode>())
            .FirstOrDefault(p => p.Name == "width");
        Assert.NotNull(param);
    }

    [Fact]
    public void ForLoop_ParsesToForLoopNode()
    {
        // Syntax: .for <var> <start> <end> [<increment>] — the increment is optional.
        string script = ".for i 1 3 1\nR\n.endfor";
        ScriptRunner.TryParse(script, out var statements, out var diag);
        Assert.False(diag.HasErrors);
        Assert.Contains(FlattenStatements(statements), s => s is ForLoopNode);
    }

    [Fact]
    public void ForLoop_WithoutIncrement_ParsesWithNullIncrement()
    {
        // The 4th argument may be omitted; the increment defaults to +/-1 at evaluation.
        string script = ".for i 1 3\nR\n.endfor";
        ScriptRunner.TryParse(script, out var statements, out var diag);
        Assert.False(diag.HasErrors);
        var loop = Assert.IsType<ForLoopNode>(FlattenStatements(statements).First(s => s is ForLoopNode));
        Assert.Null(loop.Increment);
    }

    [Fact]
    public void ForLoop_WithoutEndValue_ReportsExpectedEndValue()
    {
        // A missing end value must be caught (previously the guard wrongly re-checked start).
        string script = ".for i 1\nR\n.endfor";
        ScriptRunner.TryParse(script, out _, out var diag);
        Assert.True(diag.HasErrors);
        Assert.Contains(diag.Messages, m => m.Message.Contains("end value"));
    }

    [Fact]
    public void IfElse_ParsesToIfElseNode()
    {
        string script = ".if true\nR\n.else\nC\n.endif";
        ScriptRunner.TryParse(script, out var statements, out var diag);
        Assert.False(diag.HasErrors);
        Assert.Contains(FlattenStatements(statements), s => s is IfElseNode);
    }

    private static System.Collections.Generic.IEnumerable<SyntaxNode> FlattenStatements(ScopedStatementsNode node)
        => node.Statements.Concat(node.ControlStatements);
}
