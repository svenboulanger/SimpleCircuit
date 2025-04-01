using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.Nodes;
using SpiceSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Parser for SimpleCircuit code.
    /// </summary>
    public static class SimpleCircuitParser
    {
        /// <summary>
        /// Parses statements.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if there were no error while parsing; otherwise, <c>false</c>.</returns>
        public static bool ParseStatements(SimpleCircuitLexer lexer, ParsingContext context, out List<SyntaxNode> result)
        {
            result = [];
            bool success = true;
            while (lexer.Type != TokenType.EndOfContent)
            {
                // Ignore any newlines
                while (lexer.Type == TokenType.Newline || lexer.Type == TokenType.Comment)
                    lexer.Next();

                // Parse a statement
                if (!ParseStatement(lexer, context, out var statement))
                {
                    success = false;
                    while (lexer.Type != TokenType.Newline && lexer.Type != TokenType.EndOfContent)
                        lexer.Next();
                }    
                else if (statement is null)
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected statement"));
                    while (lexer.Type != TokenType.Newline && lexer.Type != TokenType.EndOfContent)
                        lexer.Next();
                }
                else
                    result.Add(statement);
            }
            return success;
        }

        private static bool ParseStatement(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            // Try parsing a component chain
            if (!ParseChain(lexer, context, out result))
                return false;
            if (result is not null)
                return true;

            return true;
        }

        /// <summary>
        /// Parses a component chain.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if there were no error while parsing; otherwise, <c>false</c>.</returns>
        public static bool ParseChain(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            List<SyntaxNode> items = [];

            // PinNamePin
            if (!ParsePinNamePin(lexer, context, out result))
                return false;
            if (result is null)
                return true;
            items.Add(result);

            // Alternating wires and components
            while (true)
            {
                if (lexer.Branch(TokenType.Punctuator, "<"))
                {
                    // Parse wire
                    if (!ParseWire(lexer, context, out var wire))
                        return false;
                    if (wire is null)
                    {
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected wire"));
                        lexer.SkipToTokenOrLine(TokenType.Punctuator, ">");
                        return false;
                    }

                    // '>'
                    if (!lexer.Branch(TokenType.Punctuator, ">"))
                    {
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected '>'"));
                        lexer.SkipToTokenOrLine(TokenType.Punctuator, ">");
                        return false;
                    }

                    items.Add(wire);

                    // Possibly, another pin-name-pin
                    if (!ParsePinNamePin(lexer, context, out var next))
                        return false;
                    if (next is null)
                        break;
                    items.Add(next);
                }
                else
                    break;
            }

            if (items.Count > 0)
                result = new ComponentChain(items);
            return true;
        }

        private static bool ParseWire(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            // Wire item
            List<SyntaxNode> items = [];

            while (true)
            {
                if (!ParseWireItem(lexer, context, out var arg))
                {
                    result = null;
                    return false;
                }
                if (arg is null)
                {
                    if (items.Count > 0)
                        result = new WireNode(items);
                    else
                        result = null;
                    return true;
                }
                else
                    items.Add(arg);
            }
        }

        private static bool ParseWireItem(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            // Wire direction
            if (!ParseWireDirection(lexer, context, out result))
                return false;
            if (result is not null)
                return true;

            // Variant or properties
            if (lexer.Type == TokenType.Word)
            {
                result = new IdentifierNode(lexer.Token);
                lexer.Next();

                // Property
                if (lexer.Branch(TokenType.Punctuator, "=", out var assignment))
                {
                    if (!ParseValueOrExpression(lexer, context, out var value))
                        return false;
                    if (value is null)
                    {
                        result = null;
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected value"));
                        return false;
                    }
                    result = new BinaryNode(BinaryOperatorTypes.Assignment, result, assignment, value);
                }
            }

            // Variants
            if (lexer.NextType == TokenType.Word)
            {
                if (lexer.Branch(TokenType.Punctuator, "+", out var op))
                {
                    result = new Unary(op, new IdentifierNode(lexer.Token), UnaryOperatorTypes.Positive);
                    lexer.Next();
                }
                else if (lexer.Branch(TokenType.Punctuator, "-", out op))
                {
                    result = new Unary(op, new IdentifierNode(lexer.Token), UnaryOperatorTypes.Negative);
                    lexer.Next();
                }
            }

            return true;
        }

        private static bool ParseWireDirection(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // Arrow
            if (lexer.Branch(TokenType.Arrow, out var arrow))
            {
                if (!ParseDistance(lexer, context, out var distance))
                    return false;
                result = new DirectionNode(arrow, null, distance);
            }

            // Direction letters
            else if (lexer.Type == TokenType.Word && !(lexer.NextType == TokenType.Punctuator && lexer.NextContent.ToString() == "="))
            {
                switch (lexer.Content.ToString())
                {
                    case DirectionNode.UpArrow:
                    case DirectionNode.DownArrow:
                    case DirectionNode.LeftArrow:
                    case DirectionNode.RightArrow:
                    case DirectionNode.UpLeftArrow:
                    case DirectionNode.UpRightArrow:
                    case DirectionNode.DownLeftArrow:
                    case DirectionNode.DownRightArrow:
                    case "u":
                    case "n":
                    case "d":
                    case "s":
                    case "l":
                    case "w":
                    case "r":
                    case "e":
                    case "ne":
                    case "se":
                    case "nw":
                    case "sw":
                        var dir = lexer.Token;
                        lexer.Next();

                        if (!ParseDistance(lexer, context, out var distance))
                            return false;
                        result = new DirectionNode(dir, null, distance);
                        break;

                    case "a":
                        // General angle
                        dir = lexer.Token;
                        lexer.Next();

                        // Angle
                        if (!ParseValueOrExpression(lexer, context, out var angle))
                            return false;
                        if (angle is null)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected angle"));
                            return false;
                        }

                        // Distance (optional)
                        if (!ParseDistance(lexer, context, out distance))
                            return false;
                        result = new DirectionNode(dir, angle, distance);
                        break;
                }
            }
            return true;
        }

        private static bool ParseDistance(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            if (lexer.Type == TokenType.Punctuator && lexer.NextType == TokenType.Number && lexer.Content.ToString() == "+")
            {
                // Minimum distance
                var plus = lexer.Token;
                lexer.Next();
                if (!ParseValueOrExpression(lexer, context, out var distance))
                    return false;
                result = new Minimum(distance, plus.Location);
            }
            else
            {
                if (!ParseValueOrExpression(lexer, context, out var distance))
                    return false;
                result = distance;
            }
            return true;
        }

        /// <summary>
        /// Parses a [pin]name[pin] item.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if there were no error while parsing; otherwise, <c>false</c>.</returns>
        public static bool ParsePinNamePin(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // Pin before name (optional)
            SyntaxNode pinLeft = null;
            if (lexer.Branch(TokenType.Punctuator, "["))
            {
                // Pin name
                if (!ParseName(lexer, context, out pinLeft))
                    return false;
                if (pinLeft is null)
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected pin name"));
                    lexer.SkipToTokenOrLine(TokenType.Punctuator, "]");
                    return false;
                }

                // ']'
                if (!lexer.Branch(TokenType.Punctuator, "]"))
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected ']'"));
                    lexer.SkipToTokenOrLine(TokenType.Punctuator, "]");
                    return false;
                }
            }

            // The name itself
            if (!ParseName(lexer, context, out result))
                return false;
            if (result is null)
                return true;

            // Properties (optional)
            if (lexer.Branch(TokenType.Punctuator, "("))
            {
                if (!ParsePropertyList(lexer, context, out var properties))
                {
                    lexer.SkipToTokenOrLine(TokenType.Punctuator, ")");
                    return false;
                }
                if (properties is not null)
                    result = new PropertyList(result, properties);
                if (!lexer.Branch(TokenType.Punctuator, ")"))
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected property or ')'"));
                    return false;
                }
            }

            // Pin after name (optional)
            SyntaxNode pinRight = null;
            if (lexer.Branch(TokenType.Punctuator, "["))
            {
                // Pin name
                if (!ParseName(lexer, context, out pinRight))
                    return false;
                if (pinRight is null)
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected pin name"));
                    return false;
                }

                if (pinRight is null)
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected pin name"));
                    lexer.SkipToTokenOrLine(TokenType.Punctuator, "]");
                    return false;
                }

                // ']'
                if (!lexer.Branch(TokenType.Punctuator, "]"))
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected ']'"));
                    lexer.SkipToTokenOrLine(TokenType.Punctuator, "]");
                    return false;
                }
            }

            if (pinLeft is not null || pinRight is not null)
                result = new PinNamePin(pinLeft, result, pinRight);
            return true;
        }

        private static bool ParsePropertyList(SimpleCircuitLexer lexer, ParsingContext context, out List<SyntaxNode> result)
        {
            // Property
            if (!ParseProperty(lexer, context, out var property))
            {
                result = null;
                return false;
            }
            if (property is null)
            {
                result = null;
                return true;
            }

            result = [property];
            while (true)
            {
                // ',' optional
                lexer.Branch(TokenType.Punctuator, ",");

                // Property
                if (!ParseProperty(lexer, context, out property))
                {
                    result = null;
                    return false;
                }
                if (property is null)
                    return true;
                result.Add(property);
            }
        }

        private static bool ParseProperty(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            if (lexer.Branch(TokenType.Word, out var word))
            {
                // word '=' value
                if (lexer.Branch(TokenType.Punctuator, "=", out var assignment))
                {
                    SyntaxNode value = null;
                    switch (lexer.Type)
                    {
                        case TokenType.Number:
                            if (!ExpressionParser.ParseNumber(lexer, context, out var number))
                                return false;
                            value = number;
                            break;

                        case TokenType.String:
                            result = new Quoted(lexer.Token);
                            lexer.Next();
                            break;

                        default:
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected value"));
                            return false;
                    }
                    result = new BinaryNode(BinaryOperatorTypes.Assignment, new IdentifierNode(word), assignment, value);
                }
                else
                    // word
                    result = new IdentifierNode(word);
            }
            else if (lexer.Branch(TokenType.Punctuator, "+", out var op))
            {
                // '+' word
                if (!lexer.Branch(TokenType.Word, out word))
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected variant"));
                    return false;
                }
                result = new Unary(op, new IdentifierNode(word), UnaryOperatorTypes.Positive);
                return true;
            }
            else if (lexer.Branch(TokenType.Punctuator, "-", out op))
            {
                // '-' word
                if (!lexer.Branch(TokenType.Word, out word))
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected variant"));
                    return false;
                }
                result = new Unary(op, new IdentifierNode(word), UnaryOperatorTypes.Negative);
                return true;
            }
            else if (lexer.Branch(TokenType.String, out var stringLiteral))
                result = new Quoted(stringLiteral);
            return true;
        }

        /// <summary>
        /// Parses a name.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if there were no error while parsing; otherwise, <c>false</c>.</returns>
        public static bool ParseName(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // word
            if (lexer.Branch(TokenType.Word, out var word))
                result = new Literal(word);
            else
                return true;

            // '{' expression '}'
            while (lexer.Branch(TokenType.Punctuator, "{", out var bracketLeft))
            {
                // expression
                if (!ExpressionParser.ParseExpression(lexer, context, out var expression))
                {
                    // Skip to new line or '}'
                    lexer.SkipToTokenOrLine(TokenType.Punctuator, "}");
                    return false;
                }
                if (expression is null)
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected expression"));
                    result = null;
                    return false;
                }

                // '}'
                if (!lexer.Branch(TokenType.Punctuator, "}", out var bracketRight))
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected '}'"));
                    return false;
                }

                expression = new BracketNode(bracketLeft, expression, bracketRight);
                result = new BinaryNode(BinaryOperatorTypes.Concatenate, result, default, expression);
            }
            return true;
        }

        private static bool ParseValueOrExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            switch (lexer.Type)
            {
                case TokenType.Punctuator:
                    // '{'
                    if (lexer.Branch(TokenType.Punctuator, "{"))
                    {
                        // Expression
                        if (!ExpressionParser.ParseExpression(lexer, context, out var expression))
                        {
                            lexer.SkipToTokenOrLine(TokenType.Punctuator, "}");
                            return false;
                        }

                        // '}'
                        if (!lexer.Branch(TokenType.Punctuator, "}"))
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected '}'"));
                            return false;
                        }
                    }
                    break;

                case TokenType.Number:
                    if (!ExpressionParser.ParseNumber(lexer, context, out var number))
                        return false;
                    result = number;
                    break;

                case TokenType.String:
                    result = new Quoted(lexer.Token);
                    lexer.Next();
                    break;
            }
            return true;
        }

        private static void SkipToTokenOrLine(this SimpleCircuitLexer lexer, TokenType type, string content)
        {
            while (!lexer.Branch(type, content) && lexer.Type != TokenType.Newline && lexer.Type != TokenType.EndOfContent)
                lexer.Next();
        }
    }
}
