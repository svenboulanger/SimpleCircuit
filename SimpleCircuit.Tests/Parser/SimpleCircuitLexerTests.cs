using System.Collections.Generic;
using SimpleCircuit.Parser;
using Xunit;

namespace SimpleCircuit.Tests.Parser;

public class SimpleCircuitLexerTests
{
    private static List<(TokenType Type, string Content)> Lex(string code)
    {
        var lexer = SimpleCircuitLexer.FromString(code, "test");
        var tokens = new List<(TokenType, string)>();
        while (lexer.Type != TokenType.EndOfContent)
        {
            tokens.Add((lexer.Type, lexer.Content.ToString()));
            lexer.Next();
        }
        return tokens;
    }

    [Fact]
    public void ComponentChain_ProducesWordsAndPunctuators()
    {
        var tokens = Lex("GND <u> R");
        Assert.Equal(
        [
            (TokenType.Word, "GND"),
            (TokenType.Punctuator, "<"),
            (TokenType.Word, "u"),
            (TokenType.Punctuator, ">"),
            (TokenType.Word, "R"),
        ], tokens);
    }

    [Theory]
    [InlineData("2.2k")]
    [InlineData("1meg")]
    [InlineData("1e-3")]
    [InlineData("100")]
    public void Number_IsSingleNumberToken(string number)
    {
        var tokens = Lex(number);
        Assert.Single(tokens);
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal(number, tokens[0].Content);
    }

    [Fact]
    public void QuotedString_IsSingleStringToken()
    {
        var tokens = Lex("\"hello world\"");
        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void ControlStatement_StartsWithDotPunctuator()
    {
        var tokens = Lex(".param x = 1");
        Assert.Equal(
        [
            (TokenType.Punctuator, "."),
            (TokenType.Word, "param"),
            (TokenType.Word, "x"),
            (TokenType.Punctuator, "="),
            (TokenType.Number, "1"),
        ], tokens);
    }

    [Fact]
    public void LineComment_IsTrivia()
    {
        // A '//' comment is skipped entirely; only the leading component remains.
        var tokens = Lex("R // this is a comment");
        Assert.Equal([(TokenType.Word, "R")], tokens);
    }

    [Fact]
    public void Newline_ProducesNewlineToken()
    {
        var tokens = Lex("R\nC");
        Assert.Equal(
        [
            (TokenType.Word, "R"),
            (TokenType.Newline, "\n"),
            (TokenType.Word, "C"),
        ], tokens);
    }
}
