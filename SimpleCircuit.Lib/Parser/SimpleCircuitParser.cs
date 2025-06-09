using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static bool Parse(SimpleCircuitLexer lexer, ParsingContext context, out ScopedStatementsNode result)
        {
            if (!ParseScopedStatements(lexer, context, out result))
                return false;
            if (lexer.Type != TokenType.EndOfContent)
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Unrecognized statement"));
                return false;
            }
            return true;
        }
        private static bool ParseScopedStatements(SimpleCircuitLexer lexer, ParsingContext context, out ScopedStatementsNode result, HashSet<string> excludeReferences = null)
        {
            result = null;
            var statements = new List<SyntaxNode>();
            var parameterDefinitions = new List<ParameterDefinitionNode>();
            var defaultVariantsAndProperties = new List<ControlPropertyNode>();

            while (true)
            {
                // Skip any empty lines
                while (lexer.Type == TokenType.Newline || lexer.Type == TokenType.Comment)
                    lexer.Next();
                if (lexer.Type == TokenType.EndOfContent)
                    break;

                // Parse a statement
                if (!ParseStatement(lexer, context, out var statement))
                    return false;
                if (statement is null)
                    break;

                // Don't add a statement if it is a parameter definition (this is added separately to the context)
                if (statement is ParameterDefinitionNode parameterDefinition)
                    parameterDefinitions.Add(parameterDefinition);
                else if (statement is ControlPropertyNode defaultOptions)
                    defaultVariantsAndProperties.Add(defaultOptions);
                else
                    statements.Add(statement);
            }

            // We finished parsing statements. If there are any, let's create our result node
            if (statements.Count > 0 || parameterDefinitions.Count > 0)
            {
                // Use the collected scoped statements
                foreach (var parameterDefinition in parameterDefinitions)
                    context.ReferencedVariables.Remove(parameterDefinition.Name);

                // Also exclude any parameters passed down the optional argument
                if (excludeReferences is not null)
                {
                    foreach (string name in excludeReferences)
                        context.ReferencedVariables.Remove(name);
                }
                result = new ScopedStatementsNode(statements, parameterDefinitions, defaultVariantsAndProperties, context.ReferencedVariables.OrderBy(n => n));
            }
            return true;
        }
        private static bool ParseStatement(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            // Try parsing a component chain
            if (!ParseComponentChain(lexer, context, out result))
                return false;
            if (result is not null)
                return true;

            // Try parsing a virtual chain
            if (!ParseVirtualChain(lexer, context, out result))
                return false;
            if (result is not null)
                return true;

            // Try parsing a control statement
            if (!ParseControlStatement(lexer, context, out result))
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
        public static bool ParseComponentChain(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;
            List<SyntaxNode> items = [];

            // Alternating wires and components
            while (true)
            {
                // Try parsing a component
                if (!ParsePinNamePin(lexer, context, out var item))
                    return false;
                if (item is not null)
                {
                    items.Add(item);
                    continue;
                }

                // Try parsing a wire
                if (lexer.Branch(TokenType.Punctuator, "<"))
                {
                    // Parse wire
                    SyntaxNode wire;
                    while (true)
                    {
                        if (!ParseWire(lexer, context, out wire))
                            return false;
                        if (wire is null)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected wire"));
                            lexer.SkipToTokenOrLine(TokenType.Punctuator, ">");
                            return false;
                        }

                        // Allow queued anonymous points here
                        if (lexer.Branch(TokenType.Punctuator, ".", out var dot) ||
                            context.CompatibilityMode && lexer.Branch(TokenType.Word, "x", out dot))
                        {
                            items.Add(wire);
                            items.Add(new QueuedAnonymousPoint(dot));
                        }
                        else
                            break; // We don't have an anonymous queued point
                    }

                    // '>'
                    if (!lexer.Branch(TokenType.Punctuator, ">"))
                    {
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected '>'"));
                        lexer.SkipToTokenOrLine(TokenType.Punctuator, ">");
                        return false;
                    }

                    items.Add(wire);
                }
                else if (lexer.Branch(TokenType.Punctuator, "-", out var dash))
                {
                    // Short hand for <?>
                    items.Add(new WireNode([new LiteralNode(dash)]));
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
                if (context.CompatibilityMode && lexer.Type == TokenType.Word && lexer.Content.ToString() == "x" &&
                    (lexer.NextType != TokenType.Punctuator || lexer.NextContent.ToString() != "="))
                {
                    // Compatibility mode queued anonymous point
                    return true;
                }
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
                    result = new UnaryNode(op, new IdentifierNode(lexer.Token), UnaryOperatorTypes.Positive);
                    lexer.Next();
                }
                else if (lexer.Branch(TokenType.Punctuator, "-", out op))
                {
                    result = new UnaryNode(op, new IdentifierNode(lexer.Token), UnaryOperatorTypes.Negative);
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
            else if (lexer.Type == TokenType.Punctuator)
            {
                switch (lexer.Content.ToString())
                {
                    case "?":
                    case "??":
                        result = new DirectionNode(lexer.Token, null, null);
                        lexer.Next();
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
                result = new UnaryNode(plus, distance, UnaryOperatorTypes.Positive);
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
        /// Parses a virtual chain.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if there were no error while parsing; otherwise, <c>false</c>.</returns>
        public static bool ParseVirtualChain(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;
            List<SyntaxNode> items = [];

            // '('
            if (lexer.Branch(TokenType.Punctuator, "(", out var bracketLeft))
            {
                // The constraint flags
                var flags = VirtualChainConstraints.None;
                if (lexer.Branch(TokenType.Word, "x", out var constraints))
                    flags = VirtualChainConstraints.X;
                else if (lexer.Branch(TokenType.Word, "y", out constraints))
                    flags = VirtualChainConstraints.Y;
                else if (lexer.Branch(TokenType.Word, "xy", out constraints))
                    flags = VirtualChainConstraints.XY;
                else
                    flags = VirtualChainConstraints.XY;

                // Parse the virtual chain
                while (true)
                {
                    // Virtual component pin
                    if (!ParsePinVirtualNamePin(lexer, context, out var item))
                        return false;
                    if (item is not null)
                    {
                        items.Add(item);
                        continue;
                    }

                    // Try parsing a virtual wire
                    if (lexer.Branch(TokenType.Punctuator, "<"))
                    {
                        // Parse wire
                        if (!ParseVirtualWire(lexer, context, out var wire))
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
                        continue;
                    }

                    break;
                }

                // ')'
                if (!lexer.Branch(TokenType.Punctuator, ")", out var bracketRight))
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected ')'"));
                    result = null;
                    return false;
                }
                if (items.Count > 0)
                    result = new VirtualChainNode(bracketLeft, constraints, items, bracketRight, flags);
            }
            return true;
        }

        private static bool ParseVirtualWire(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            // Wire item
            List<SyntaxNode> items = [];

            while (true)
            {
                if (!ParseVirtualWireItem(lexer, context, out var arg))
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
        private static bool ParseVirtualWireItem(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            // Wire direction
            if (!ParseWireDirection(lexer, context, out result))
                return false;
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
                if (!ParseName(lexer, context, out pinLeft, allowNumbers: true))
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
                    result = new PropertyListNode(result, properties);
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
                if (!ParseName(lexer, context, out pinRight, allowNumbers: true))
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
                result = new PinNamePinNode(pinLeft, result, pinRight);
            return true;
        }

        /// <summary>
        /// Parses a [pin]virtual-name[pin] item.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if there were no error while parsing; otherwise, <c>false</c>.</returns>
        public static bool ParsePinVirtualNamePin(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // Pin before name (optional)
            SyntaxNode pinLeft = null;
            if (lexer.Branch(TokenType.Punctuator, "["))
            {
                // Pin name
                if (!ParseName(lexer, context, out pinLeft, allowNumbers: true))
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
            if (!ParseFilterName(lexer, context, out result))
                return false;
            if (result is null)
                return true;

            // Pin after name (optional)
            SyntaxNode pinRight = null;
            if (lexer.Branch(TokenType.Punctuator, "["))
            {
                // Pin name
                if (!ParseName(lexer, context, out pinRight, allowNumbers: true))
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
                result = new PinNamePinNode(pinLeft, result, pinRight);
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
                    if (!ParseValueOrExpression(lexer, context, out var value))
                        return false;
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
                result = new UnaryNode(op, new LiteralNode(word), UnaryOperatorTypes.Positive);
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
                result = new UnaryNode(op, new LiteralNode(word), UnaryOperatorTypes.Negative);
                return true;
            }
            else if (lexer.Branch(TokenType.String, out var stringLiteral))
                result = new QuotedNode(stringLiteral);
            return true;
        }

        /// <summary>
        /// Parses a name.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if there were no error while parsing; otherwise, <c>false</c>.</returns>
        public static bool ParseName(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result, bool allowNumbers = false)
        {
            result = null;
            while (true)
            {
                // We don't allow spaces for concatenations
                if (result is not null && lexer.HasTrivia)
                    return true;

                if (allowNumbers && lexer.Branch(TokenType.Number, out var number))
                {
                    // A number (if allowed)
                    var expression = new LiteralNode(number);
                    result = result is null ? expression : new BinaryNode(BinaryOperatorTypes.Concatenate, result, default, expression);
                }
                else if (result is not null && lexer.Branch(TokenType.Punctuator, "/", out var opSlash))
                {
                    // A separator for sections
                    result = new BinaryNode(BinaryOperatorTypes.Concatenate, result, default, new LiteralNode(opSlash));
                }
                else if (lexer.Branch(TokenType.Word, out var word))
                {
                    // A word
                    var expression = new LiteralNode(word);
                    result = result is null ? expression : new BinaryNode(BinaryOperatorTypes.Concatenate, result, default, expression);
                }
                else if (lexer.Branch(TokenType.Punctuator, "{", out var bracketLeft))
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
                    result = result is null ? expression : new BinaryNode(BinaryOperatorTypes.Concatenate, result, default, expression);
                }
                else
                    return true;
            }
        }

        /// <summary>
        /// Parses name that can contain wildcard characters for filtering.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if there were no error while parsing; otherwise, <c>false</c>.</returns>
        public static bool ParseFilterName(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;
            while (true)
            {
                // Don't allow concatenation if there's any spaces between it
                if (result is not null && lexer.HasTrivia)
                    return true;

                // word
                if (result is not null && lexer.Branch(TokenType.Punctuator, "/", out var opSlash))
                {
                    // A separator for sections
                    result = new BinaryNode(BinaryOperatorTypes.Concatenate, result, default, new LiteralNode(opSlash));
                }
                else if (lexer.Branch(TokenType.Word, out var word))
                {
                    // A word
                    var expression = new LiteralNode(word);
                    result = result is null ? expression : new BinaryNode(BinaryOperatorTypes.Concatenate, result, default, expression);
                }
                else if (lexer.Branch(TokenType.Punctuator, "{", out var bracketLeft))
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
                    result = result is null ? expression : new BinaryNode(BinaryOperatorTypes.Concatenate, result, default, expression);
                }
                else if (lexer.Branch(TokenType.Punctuator, "*", out var asterisk))
                    result = result is null ? new LiteralNode(asterisk) : new BinaryNode(BinaryOperatorTypes.Concatenate, result, default, new LiteralNode(asterisk));
                else
                    return true;
            }
        }

        private static bool ParseValueOrExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            switch (lexer.Type)
            {
                case TokenType.Punctuator:
                    // '{'
                    if (lexer.Branch(TokenType.Punctuator, "{", out var bracketLeft))
                    {
                        // Expression
                        if (!ExpressionParser.ParseExpression(lexer, context, out var expression))
                        {
                            lexer.SkipToTokenOrLine(TokenType.Punctuator, "}");
                            return false;
                        }

                        // '}'
                        if (!lexer.Branch(TokenType.Punctuator, "}", out var bracketRight))
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token, SeverityLevel.Error, "ERR", "Expected '}'"));
                            return false;
                        }

                        result = new BracketNode(bracketLeft, expression, bracketRight);
                    }
                    else if (lexer.Branch(TokenType.Punctuator, "-", out var minus))
                    {
                        // parse a number
                        if (!ParseValueOrExpression(lexer, context, out var argument))
                            return false;
                        result = new UnaryNode(minus, argument, UnaryOperatorTypes.Negative);
                    }
                    else if (lexer.Branch(TokenType.Punctuator, "+", out var plus))
                    {
                        if (!ParseValueOrExpression(lexer, context, out var argument))
                            return false;
                        result = new UnaryNode(plus, argument, UnaryOperatorTypes.Positive);
                    }
                    break;

                case TokenType.Number:
                    if (!ExpressionParser.ParseNumber(lexer, context, out var number))
                        return false;
                    result = number;
                    break;

                case TokenType.Word:
                    switch (lexer.Content.ToString())
                    {
                        case "true":
                        case "false":
                            result = new LiteralNode(lexer.Token);
                            return true;

                        default:
                            result = null;
                            break;
                    }
                    break;

                case TokenType.String:
                    result = new QuotedNode(lexer.Token);
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

        /// <summary>
        /// Parses a control statement.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if there were no error while parsing; otherwise, <c>false</c>.</returns>
        public static bool ParseControlStatement(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // '.' word (without trivia)
            if (lexer.Type == TokenType.Punctuator && lexer.Content.ToString() == "." &&
                lexer.NextType == TokenType.Word && lexer.NextHasTrivia == false)
            {
                var word = lexer.NextToken;
                switch (lexer.NextContent.ToString())
                {
                    case "box":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'box'
                        if (!ParseBox(word, lexer, context, out result))
                            return false;
                        break;

                    case "variant":
                    case "variants":
                    case "property":
                    case "properties":
                        lexer.Next(); // '.'
                        lexer.Next(); // the control statement word
                        if (!ParseControlPropertyStatement(lexer, context, out result))
                            return false;
                        break;

                    case "param":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'param'
                        if (!ParseParameterDefinition(word, lexer, context, out result))
                            return false;
                        break;

                    case "scope":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'scope'
                        if (!ParseScopeDefinition(word, lexer, context, out result))
                            return false;
                        if (result is null)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected scope definition"));
                            return false;
                        }
                        break;

                    case "section":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'section'
                        if (!ParseSectionDefinition(word, lexer, context, out result))
                            return false;
                        if (result is null)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected section definition"));
                            return false;
                        }
                        break;

                    case "for":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'for'
                        if (!ParseForLoop(word, lexer, context, out result))
                            return false;
                        if (result is null)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected for loop"));
                            return false;
                        }
                        break;

                    case "symbol":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'symbol'
                        if (!ParseSymbolDefinition(lexer, context, out result))
                            return false;
                        if (result is null)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected symbol definition"));
                            return false;
                        }
                        break;

                    case "if":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'if'
                        if (!ParseIfElse(word, lexer, context, out result))
                            return false;
                        if (result is null)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected if-else statement"));
                            return false;
                        }
                        break;

                    case "subckt":
                        if (!context.AllowSubcircuitDefinitions)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Nested subcircuit definitions are not allowed"));
                            return false;
                        }
                        lexer.Next(); // '.'
                        lexer.Next(); // 'subckt'
                        if (!ParseSubcircuitDefinition(word, lexer, context, out result))
                            return false;
                        if (result is null)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected subcircuit definition"));
                            return false;
                        }
                        break;

                    default:
                        break;
                }
            }
            return true;
        }
        private static bool ParseControlPropertyStatement(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;
            if (!ParseFilterName(lexer, context, out var name))
                return false;

            // Start reading properties
            if (!ParsePropertyList(lexer, context, out var properties))
                return false;
            result = new ControlPropertyNode(name, properties);
            return true;
        }
        private static bool ParseParameterDefinition(Token param, SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;
            if (lexer.Branch(TokenType.Word, out var identifier))
            {
                if (!lexer.Branch(TokenType.Punctuator, "="))
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected a '='"));
                    return false;
                }
                if (!ParseValueOrExpression(lexer, context, out var value))
                    return false;
                if (value is null)
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected a value"));
                    return false;
                }
                result = new ParameterDefinitionNode(param, identifier, value);
            }
            return true;
        }
        private static bool ParseBox(Token box, SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // Properties and variants
            if (!ParsePropertyList(lexer, context, out var propertyList))
                return false;

            // Start the statements
            if (!lexer.Branch(TokenType.Newline))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected new line"));
                return false;
            }

            // Parse the statements
            if (!ParseScopedStatements(lexer, context, out var statements))
                return false;

            // Expect a .ends
            if (lexer.Type == TokenType.Punctuator && lexer.Content.ToString() == ".")
            {
                if (lexer.NextType == TokenType.Word && !lexer.NextHasTrivia)
                {
                    switch (lexer.NextContent.ToString())
                    {
                        case "endb":
                        case "endbox":
                            lexer.Next(); // '.'
                            lexer.Next(); // 'ends'

                            result = new BoxNode(box, propertyList, statements);
                            return true;
                    }
                }
            }
            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected '.ends' or '.endscope'"));
            return false;
        }
        private static bool ParseScopeDefinition(Token scope, SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // Properties
            if (!ParsePropertyList(lexer, context, out var propertyList))
                return false;

            // Start the statements
            if (!lexer.Branch(TokenType.Newline))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected new line"));
                return false;
            }

            // Parse the statements
            if (!ParseScopedStatements(lexer, context, out var statements))
                return false;

            // Expect a .ends
            if (lexer.Type == TokenType.Punctuator && lexer.Content.ToString() == ".")
            {
                if (lexer.NextType == TokenType.Word && !lexer.NextHasTrivia)
                {
                    switch (lexer.NextContent.ToString())
                    {
                        case "ends":
                        case "endscope":
                            lexer.Next(); // '.'
                            lexer.Next(); // 'ends'

                            result = new ScopeDefinitionNode(scope, propertyList, statements);
                            return true;
                    }
                }
            }
            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected '.ends' or '.endscope'"));
            return false;
        }
        private static bool ParseSectionDefinition(Token section, SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // Section name
            if (!lexer.Branch(TokenType.Word, out var nameToken))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected a section name"));
                return false;
            }

            // Expected properties
            if (!ParsePropertyList(lexer, context, out var propertyList))
                return false;

            // If the properties indicate a templated section, don't try to look for statements
            if (propertyList is not null && propertyList.Count > 0 && propertyList[0] is IdentifierNode)
            {
                result = new SectionDefinitionNode(section, nameToken, propertyList, ScopedStatementsNode.Empty);
                return true;
            }

            // Start the statements
            if (!lexer.Branch(TokenType.Newline))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected new line"));
                return false;
            }

            // Parse the statements
            if (!ParseScopedStatements(lexer, context, out var statements))
                return false;

            // Expect a .ends
            if (lexer.Type == TokenType.Punctuator && lexer.Content.ToString() == ".")
            {
                if (lexer.NextType == TokenType.Word && !lexer.NextHasTrivia)
                {
                    switch (lexer.NextContent.ToString())
                    {
                        case "ends":
                        case "endsection":
                            lexer.Next(); // '.'
                            lexer.Next(); // 'ends'

                            result = new SectionDefinitionNode(section, nameToken, propertyList, statements);
                            return true;
                    }
                }
            }
            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected '.ends' or '.endsection'"));
            return false;
        }
        private static bool ParseForLoop(Token @for, SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // Parse the variable name
            if (!lexer.Branch(TokenType.Word, out var variableToken))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected a variable name"));
                return false;
            }

            // Parse the initializer expression
            if (!ParseValueOrExpression(lexer, context, out var start))
                return false;
            if (start is null)
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected start value"));
                return false;
            }

            // Parse the end value
            if (!ParseValueOrExpression(lexer, context, out var end))
                return false;
            if (start is null)
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected end value"));
                return false;
            }

            // Parse the increment value
            if (!ParseValueOrExpression(lexer, context, out var increment))
                return false;
            if (increment is null)
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected increment"));
                return false;
            }

            // New line
            if (!lexer.Branch(TokenType.Newline))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected a new line"));
                return false;
            }

            // Parse statements
            if (!ParseScopedStatements(lexer, context, out var statements, [variableToken.Content.ToString()]))
                return false;

            // Expect the end of the loop
            if (lexer.Type == TokenType.Punctuator && lexer.NextType == TokenType.Word && !lexer.NextHasTrivia &&
                lexer.Content.ToString() == ".")
            {
                switch (lexer.NextContent.ToString())
                {
                    case "end":
                    case "endf":
                    case "endfor":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'end'
                        result = new ForLoopNode(@for, variableToken, start, end, increment, statements);
                        return true;
                }
            }
            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected for-loop end ('.end', '.endf', '.endfor')"));
            return false;
        }
        private static bool ParseSymbolDefinition(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            // Read the key name
            if (!lexer.Branch(TokenType.Word, out var keyToken))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected symbol key"));
                return false;
            }
            if (!lexer.Branch(TokenType.Newline))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected a newline"));
                return false;
            }

            // Read the XML
            var tracker = lexer.Track();
            while (lexer.Type != TokenType.EndOfContent)
            {
                if (lexer.Type == TokenType.Punctuator && lexer.NextType == TokenType.Word &&
                    lexer.Content.ToString() == ".")
                {
                    switch (lexer.NextContent.ToString())
                    {
                        case "end":
                        case "ends":
                        case "endsymbol":
                            // Get the XML tracked
                            var xml = lexer.GetTracked(tracker);

                            // Consume the end
                            lexer.Next(); // '.'
                            lexer.Next(); // 'end':

                            // Try to parse the XML
                            var doc = new XmlDocument();
                            try
                            {
                                string fullXml = $"<symbol>{Environment.NewLine}{xml.Content}{Environment.NewLine}</symbol>";
                                doc.LoadXml(fullXml);
                            }
                            catch (XmlException ex)
                            {
                                var loc = new TextLocation(
                                    xml.Location.Source,
                                    xml.Location.Line + ex.LineNumber - 1,
                                    xml.Location.Line == 1 ? xml.Location.Column + ex.LinePosition : ex.LinePosition);
                                context.Diagnostics?.Post(new SourceDiagnosticMessage(loc, SeverityLevel.Error, "XML", ex.Message));
                                return false;
                            }
                            result = new SymbolDefinitionNode(keyToken, doc.DocumentElement);
                            return true;
                    }
                }
                lexer.Next();
            }
            if (lexer.Type == TokenType.EndOfContent)
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Unexpected end of document, expected symbol end"));
                return false;
            }
            return true;
        }
        private static bool ParseIfElse(Token @if, SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;
            List<IfConditionNode> conditions = [];
            ScopedStatementsNode elseStatements = null;

            // Parse the condition
            if (!ParseValueOrExpression(lexer, context, out var condition))
                return false;
            if (condition is null)
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected condition"));
                return false;
            }
            if (!lexer.Branch(TokenType.Newline))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected end of line"));
                return false;
            }

            // Parse the statements
            if (!ParseScopedStatements(lexer, context, out var statements))
                return false;
            conditions.Add(new IfConditionNode(@if, condition, statements));

            // Parse elif statements
            while (lexer.Type == TokenType.Punctuator && lexer.NextType == TokenType.Word &&
                lexer.Content.ToString() == ".")
            {
                switch (lexer.NextContent.ToString())
                {
                    case "end":
                    case "endif":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'endif'
                        result = new IfElseNode(conditions, elseStatements);
                        return true;

                    case "elif":
                        lexer.Next(); // '.'
                        @if = lexer.Token;
                        lexer.Next(); // 'elif'

                        // Condition
                        if (!ParseValueOrExpression(lexer, context, out condition))
                            return false;
                        if (condition is null)
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected condition"));
                            return false;
                        }
                        if (!lexer.Branch(TokenType.Newline))
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected end of line"));
                            return false;
                        }

                        // Statements
                        if (!ParseScopedStatements(lexer, context, out statements))
                            return false;
                        conditions.Add(new IfConditionNode(@if, condition, statements));
                        break;

                    case "else":
                        lexer.Next(); // '.'
                        lexer.Next(); // 'else'
                        if (!lexer.Branch(TokenType.Newline))
                        {
                            context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected end of line"));
                            return false;
                        }

                        // Statements
                        if (!ParseScopedStatements(lexer, context, out elseStatements))
                            return false;
                        break;

                    default:
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", $"Could not recognize control statement '.{lexer.NextContent}'"));
                        return false;
                }
            }
            return true;
        }
        private static bool ParseSubcircuitDefinition(Token subckt, SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;
            if (!lexer.Branch(TokenType.Word, out var nameToken))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected subcircuit name"));
                return false;
            }

            // Try parsing pins
            List<SyntaxNode> pins = [];
            while (lexer.Check(~TokenType.Newline))
            {
                // Detect properties
                if (lexer.Type == TokenType.Word && lexer.NextType == TokenType.Punctuator && lexer.NextContent.ToString() == "=")
                    break;

                // Parse the pin
                if (!ParsePinNamePin(lexer, context, out var pin))
                    return false;
                if (pin is null)
                {
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected a pin"));
                    return false;
                }
                pins.Add(pin);
            }
            List<SyntaxNode> properties = null;
            if (lexer.Check(~TokenType.Newline))
            {
                if (!ParsePropertyList(lexer, context, out properties))
                    return false;
            }

            if (!lexer.Branch(TokenType.Newline))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(lexer.Token.Location, SeverityLevel.Error, "ERR", "Expected a new line"));
                return false;
            }

            // Parse the statements
            bool lastAllowSubckt = context.AllowSubcircuitDefinitions;
            context.AllowSubcircuitDefinitions = false;
            if (!ParseScopedStatements(lexer, context, out var statements))
                return false;
            context.AllowSubcircuitDefinitions = lastAllowSubckt;

            // Expect a .ends
            if (lexer.Type == TokenType.Punctuator && lexer.Content.ToString() == ".")
            {
                if (lexer.NextType == TokenType.Word && !lexer.NextHasTrivia)
                {
                    switch (lexer.NextContent.ToString())
                    {
                        case "ends":
                        case "endsubckt":
                            lexer.Next(); // '.'
                            lexer.Next(); // 'ends'

                            result = new SubcircuitDefinitionNode(subckt, nameToken, pins, properties, statements);
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
