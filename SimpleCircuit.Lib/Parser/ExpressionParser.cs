using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Methods for parsing expressions.
    /// </summary>
    public static class ExpressionParser
    {
        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if no errors were encountered; otherwise, <c>false</c>.</returns>
        public static bool ParseExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            if (!ParseConditionalExpression(lexer, context, out result))
                return false;

            // If there are more arguments, 
            if (lexer.Check(TokenType.Punctuator, ","))
            {
                // Read second argument
                var args = new List<SyntaxNode> { result };

                // Read any extra arguments
                while (lexer.Branch(TokenType.Punctuator, ","))
                {
                    if (!ParseConditionalExpression(lexer, context, out result))
                        return false;
                    args.Add(result);
                }
                result = new VectorNode(args);
            }
            return true;
        }

        private static bool ParseConditionalExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            if (!ParseLogicalOrExpression(lexer, context, out result))
                return false;
            if (result is null)
                return true;

            if (lexer.Branch(TokenType.Punctuator, "?"))
            {
                // Expression if true
                if (!ParseExpression(lexer, context, out var ifTrue))
                    return false;
                if (ifTrue is null)
                {
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedExpression);
                    return false;
                }

                // ':'
                if (!lexer.Branch(TokenType.Punctuator, ":"))
                {
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedGeneric, ":");
                    return false;
                }

                // Conditional expression if false
                if (!ParseConditionalExpression(lexer, context, out var ifFalse))
                    return false;
                if (ifFalse is null)
                {
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedExpression);
                    return false;
                }
                result = new TernaryNode(result, ifTrue, ifFalse);
            }
            return true;
        }
        private static bool ParseLogicalOrExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "||" => BinaryOperatorTypes.LogicalOr,
                    _ => BinaryOperatorTypes.None
                }, ParseLogicalAndExpression);
        }
        private static bool ParseLogicalAndExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "&&" => BinaryOperatorTypes.LogicalAnd,
                    _ => BinaryOperatorTypes.None
                }, ParseOrExpression);
        }
        private static bool ParseOrExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "|" => BinaryOperatorTypes.Or,
                    _ => BinaryOperatorTypes.None
                }, ParseXorExpression);
        }
        private static bool ParseXorExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "^" => BinaryOperatorTypes.Xor,
                    _ => BinaryOperatorTypes.None
                }, ParseAndExpression);
        }
        private static bool ParseAndExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "&" => BinaryOperatorTypes.And,
                    _ => BinaryOperatorTypes.None
                }, ParseEqualityExpression);
        }
        private static bool ParseEqualityExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "==" => BinaryOperatorTypes.Equals,
                    "!=" => BinaryOperatorTypes.NotEquals,
                    _ => BinaryOperatorTypes.None
                },
                ParseRelationalExpression);
        }
        private static bool ParseRelationalExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "<" => BinaryOperatorTypes.SmallerThan,
                    ">" => BinaryOperatorTypes.GreaterThan,
                    "<=" => BinaryOperatorTypes.SmallerThanOrEqual,
                    ">=" => BinaryOperatorTypes.GreaterThanOrEqual,
                    _ => BinaryOperatorTypes.None
                },
                ParseShiftExpression);
        }
        private static bool ParseShiftExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "<<" => BinaryOperatorTypes.ShiftLeft,
                    ">>" => BinaryOperatorTypes.ShiftRight,
                    _ => BinaryOperatorTypes.None
                },
                ParseAdditiveExpression);
        }
        private static bool ParseAdditiveExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "+" => BinaryOperatorTypes.Addition,
                    "-" => BinaryOperatorTypes.Subtraction,
                    _ => BinaryOperatorTypes.None
                },
                ParseMultipliciativeExpression);
        }
        private static bool ParseMultipliciativeExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            return ParseBinaryOperator(lexer, context, out result,
                op => op switch
                {
                    "*" => BinaryOperatorTypes.Multiplication,
                    "/" => BinaryOperatorTypes.Division,
                    "%" => BinaryOperatorTypes.Modulo,
                    _ => BinaryOperatorTypes.None
                },
                ParseUnaryExpression);
        }
        private static bool ParseUnaryExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;

            if (lexer.Type == TokenType.Punctuator)
            {
                var token = lexer.Token;
                string op = token.Content.ToString();
                var type = op switch
                {
                    "+" => UnaryOperatorTypes.Positive,
                    "-" => UnaryOperatorTypes.Negative,
                    "!" => UnaryOperatorTypes.Invert,
                    "++" => UnaryOperatorTypes.PrefixIncrement,
                    "--" => UnaryOperatorTypes.PrefixDecrement,
                    _ => UnaryOperatorTypes.None
                };

                if (type != UnaryOperatorTypes.None)
                {
                    lexer.Next(); // operator
                    if (!ParsePostfixExpression(lexer, context, out var arg))
                        return false;
                    if (arg is null)
                    {
                        context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedExpression);
                        return false;
                    }
                    result = new UnaryNode(token, arg, type);
                    return true;
                }
            }

            // Can only be a primary expression
            if (!ParsePostfixExpression(lexer, context, out result))
                return false;
            return true;
        }
        private static bool ParsePostfixExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            // Primary expression
            if (!ParsePrimaryExpression(lexer, context, out result))
                return false;
            if (result is null)
                return true;

            while (true)
            {
                if (lexer.Branch(TokenType.Punctuator, "("))
                {
                    // Parse arguments
                    List<SyntaxNode> args = [];
                    if (!ParseExpression(lexer, context, out var arg))
                        return false;
                    if (arg is not null)
                    {
                        args.Add(arg);
                        while (lexer.Branch(TokenType.Punctuator, ","))
                        {
                            if (!ParseExpression(lexer, context, out arg))
                                return false;
                            if (arg is null)
                            {
                                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedExpression);
                                return false;
                            }
                            args.Add(arg);
                        }
                    }
                }
                else if (lexer.Branch(TokenType.Punctuator, "++", out var op))
                    result = new UnaryNode(op, result, UnaryOperatorTypes.PostfixIncrement);
                else if (lexer.Branch(TokenType.Punctuator, "--", out op))
                    result = new UnaryNode(op, result, UnaryOperatorTypes.PostfixDecrement);
                else
                    return true;
            }
        }
        private static bool ParsePrimaryExpression(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result)
        {
            result = null;
            switch (lexer.Type)
            {
                case TokenType.Word:
                    // Special keywords
                    switch (lexer.Content.ToString())
                    {
                        case "true":
                            result = new ConstantNode(true, lexer.NextToken);
                            lexer.Next();
                            return true;

                        case "false":
                            result = new ConstantNode(false, lexer.NextToken);
                            lexer.Next();
                            return true;
                    }

                    // Variable or function
                    if (lexer.NextType == TokenType.Punctuator && lexer.NextContent.Length == 1 && lexer.NextContent.Span[0] == '(')
                    {
                        // Function
                        var id = lexer.Token;
                        lexer.Next();
                        lexer.Next(); // '('

                        if (!lexer.Branch(TokenType.Punctuator, ")", out var bracketClose))
                        {
                            if (!ParseExpression(lexer, context, out var argument))
                                return false;
                            
                            if (!lexer.Branch(TokenType.Punctuator, ")"))
                            {
                                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedGeneric, ")");
                                return false;
                            }
                            result = new CallNode(new IdentifierNode(id), argument);
                        }
                        else
                            result = new CallNode(new IdentifierNode(id), null);
                    }
                    else
                    {
                        // Variable
                        result = new IdentifierNode(lexer.Token);
                        context.ReferencedVariables.Add(lexer.Content.ToString());
                        lexer.Next();
                    }
                    return true;

                case TokenType.String:
                    result = new QuotedNode(lexer.Token);
                    lexer.Next();
                    return true;

                case TokenType.Number:
                    if (!ParseNumber(lexer, context, out var number))
                        return false;
                    result = number;
                    return true;

                case TokenType.Punctuator:
                    // '('
                    if (lexer.Branch(TokenType.Punctuator, "(", out var bracketLeft))
                    {
                        // Expression
                        if (!ParseExpression(lexer, context, out result))
                            return false;
                        if (result is null)
                        {
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedExpression);
                            result = null;
                            return false;
                        }

                        // ')'
                        if (!lexer.Branch(TokenType.Punctuator, ")", out var bracketRight))
                        {
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedGeneric, ")");
                            return false;
                        }

                        result = new BracketNode(bracketLeft, result, bracketRight);
                    }
                    break;
            }
            return true;
        }

        private delegate bool ParseMethod(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result);

        private static bool ParseBinaryOperator(SimpleCircuitLexer lexer, ParsingContext context, out SyntaxNode result,
            Func<string, BinaryOperatorTypes> getOperatorType,
            ParseMethod subMethod)
        {
            // Sub method
            if (!subMethod(lexer, context, out result))
                return false;
            if (result is null)
                return true;

            while (true)
            {
                if (lexer.Type == TokenType.Punctuator)
                {
                    var token = lexer.Token;
                    string op = token.Content.ToString();
                    var type = getOperatorType(op);

                    if (type != BinaryOperatorTypes.None)
                    {
                        lexer.Next(); // operator
                        if (!subMethod(lexer, context, out var right))
                            return false;
                        if (right is null)
                        {
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedExpression);
                            return false;
                        }
                        result = new BinaryNode(type, result, token, right);
                    }
                    else
                        return true;
                }
                else
                    return true;
            }
        }

        /// <summary>
        /// Parses a number.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if no errors were encountered; otherwise, <c>false</c>.</returns>
        public static bool ParseNumber(SimpleCircuitLexer lexer, ParsingContext context, out ConstantNode result)
        {
            // Should be a number token
            if (lexer.Type != TokenType.Number)
            {
                result = null;
                return true;
            }

            // Parse the number
            var location = lexer.Token.Location;
            var span = lexer.Content.Span;
            int i = 0;
            for (; i < span.Length; i++)
            {
                if (span[i] == 'e' || span[i] == 'E')
                {
                    if (i + 1 < span.Length &&
                        (span[i + 1] == '+' || span[i + 1] == '-' || char.IsDigit(span[i + 1])))
                    {
                        // This is an exponential part
                        i += 2;
                        while (i < span.Length && char.IsDigit(span[i]))
                            i++;
                    }
                }
                else if (!char.IsDigit(span[i]) && span[i] != '.')
                    break;
            }

            // We now have two parts that we will parse
            double scalar = double.Parse(lexer.Content.ToString()[..i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

            // Postfix modifier
            if (i < span.Length)
            {
                switch (span[i])
                {
                    case 'a':
                    case 'A': scalar *= 1e-18; break;
                    case 'f':
                    case 'F': scalar *= 1e-15; break;
                    case 'p':
                    case 'P': scalar *= 1e-12; break;
                    case 'n':
                    case 'N': scalar *= 1e-9; break;
                    case 'u':
                    case 'U': scalar *= 1e-6; break;
                    case 'm':
                    case 'M':
                        // Here, it might also be meg
                        if (i + 2 < span.Length && (span[i + 1] == 'e' || span[i + 1] == 'E') && (span[i + 2] == 'g' || span[i + 2] == 'G'))
                            scalar *= 1e6;
                        else
                            scalar *= 1e-3;
                        break;
                    case 'k':
                    case 'K': scalar *= 1e3; break;
                    case 'g':
                    case 'G': scalar *= 1e9; break;
                    case 't':
                    case 'T': scalar *= 1e12; break;
                }
            }

            result = new ConstantNode(scalar, lexer.Token);
            lexer.Next();
            return true;
        }
    }
}
