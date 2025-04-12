using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.Nodes;
using SpiceSharp;
using System;

namespace SimpleCircuit.Evaluator
{
    /// <summary>
    /// Evaluator for expressions.
    /// </summary>
    public static class ExpressionEvaluator
    {
        public static object Evaluate(SyntaxNode node, EvaluationContext context)
        {
            return node switch
            {
                BinaryNode binary => Evaluate(binary, context),
                CallNode call => Evaluate(call, context),
                IdentifierNode id => Evaluate(id, context),
                NumberNode number => Evaluate(number, context),
                QuotedNode quoted => Evaluate(quoted, context),
                TernaryNode ternary => Evaluate(ternary, context),
                UnaryNode unary => Evaluate(unary, context),
                BracketNode bracket => Evaluate(bracket, context),
                _ => throw new NotImplementedException(),
            };
        }

        private static object Evaluate(BracketNode bracket, EvaluationContext context)
        {
            if (bracket.Left.Content.Length == 1 && bracket.Right.Content.Length == 1 &&
                bracket.Left.Content.Span[0] == '(' && bracket.Right.Content.Span[0] == ')')
                return Evaluate(bracket.Value, context);
            context.Diagnostics?.Post(new SourceDiagnosticMessage(bracket.Left.Location, SeverityLevel.Error, "ERR", "Unrecognized brackets"));
            return null;
        }
        private static object Evaluate(BinaryNode binary, EvaluationContext context)
        {
            object left = Evaluate(binary.Left, context);
            object right = Evaluate(binary.Right, context);
            if (left is null || right is null)
                return null;

            switch (binary.Type)
            {
                case BinaryOperatorTypes.Addition:
                    {
                        if (left is double dLeft)
                        {
                            if (right is double dRight)
                                return dLeft + dRight;
                            return dLeft.GetString() + right.ToString();
                        }
                        else
                        {
                            if (right is double dRight)
                                return left.ToString() + dRight.GetString();
                            return left.ToString() + right.ToString();
                        }
                    }

                case BinaryOperatorTypes.Subtraction:
                    {
                        if (left is double dLeft && right is double dRight)
                            return dLeft - dRight;
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot subtract non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.Multiplication:
                    {
                        if (left is double dLeft && right is double dRight)
                            return dLeft * dRight;
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot multiply non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.Division:
                    {
                        if (left is double dLeft && right is double dRight)
                            return dLeft / dRight;
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot divide non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.Modulo:
                    {
                        if (left is double dLeft && right is double dRight)
                            return dLeft % dRight;
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot modulo non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.LogicalAnd:
                    {
                        if (left is double dLeft && right is double dRight)
                            return !left.Equals(0.0) && !right.Equals(0.0);
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot logical-AND non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.LogicalOr:
                    {
                        if (left is double dLeft && right is double dRight)
                            return !left.Equals(0.0) || !right.Equals(0.0);
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot logical-OR non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.And:
                    {
                        if (left is double dLeft && right is double dRight)
                        {
                            int l = (int)Math.Round(dLeft, MidpointRounding.AwayFromZero);
                            int r = (int)Math.Round(dRight, MidpointRounding.AwayFromZero);
                            return (double)(l & r);
                        }
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot AND non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.Or:
                    {
                        if (left is double dLeft && right is double dRight)
                        {
                            int l = (int)Math.Round(dLeft, MidpointRounding.AwayFromZero);
                            int r = (int)Math.Round(dRight, MidpointRounding.AwayFromZero);
                            return (double)(l | r);
                        }
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot OR non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.Xor:
                    {
                        if (left is double dLeft && right is double dRight)
                        {
                            int l = (int)Math.Round(dLeft, MidpointRounding.AwayFromZero);
                            int r = (int)Math.Round(dRight, MidpointRounding.AwayFromZero);
                            return (double)(l ^ r);
                        }
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot XOR non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.Equals:
                    {
                        if (left is double dLeft && right is double dRight)
                            return dLeft.Equals(dRight);
                        else
                            return left.ToString() == right.ToString();
                    }

                case BinaryOperatorTypes.NotEquals:
                    {
                        if (left is double dLeft && right is double dRight)
                            return !dLeft.Equals(dRight);
                        else
                            return left.ToString() != right.ToString();
                    }

                case BinaryOperatorTypes.SmallerThan:
                    {
                        if (left is double dLeft && right is double dRight)
                            return dLeft < dRight;
                        else
                            return left.ToString().CompareTo(right.ToString()) < 0;
                    }

                case BinaryOperatorTypes.GreaterThan:
                    {
                        if (left is double dLeft && right is double dRight)
                            return dLeft > dRight;
                        else
                            return left.ToString().CompareTo(right.ToString()) > 0;
                    }

                case BinaryOperatorTypes.SmallerThanOrEqual:
                    {
                        if (left is double dLeft && right is double dRight)
                            return dLeft <= dRight;
                        else
                            return left.ToString().CompareTo(right.ToString()) <= 0;
                    }

                case BinaryOperatorTypes.GreaterThanOrEqual:
                    {
                        if (left is double dLeft && right is double dRight)
                            return dLeft >= dRight;
                        else
                            return left.ToString().CompareTo(right.ToString()) >= 0;
                    }

                case BinaryOperatorTypes.ShiftLeft:
                    {
                        if (left is double dLeft && right is double dRight)
                        {
                            int l = (int)Math.Round(dLeft, MidpointRounding.AwayFromZero);
                            int r = (int)Math.Round(dRight, MidpointRounding.AwayFromZero);
                            return (double)(l << r);
                        }
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot shift-left non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.ShiftRight:
                    {
                        if (left is double dLeft && right is double dRight)
                        {
                            int l = (int)Math.Round(dLeft, MidpointRounding.AwayFromZero);
                            int r = (int)Math.Round(dRight, MidpointRounding.AwayFromZero);
                            return (double)(l >> r);
                        }
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", "Cannot shift-right non-numbers"));
                        return null;
                    }

                case BinaryOperatorTypes.Concatenate:
                    return left.ToString() + right.ToString();

                default:
                    throw new NotImplementedException();
            }
        }
        private static object Evaluate(CallNode call, EvaluationContext context)
        {
            object[] args = new object[call.Arguments.Length];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Evaluate(call.Arguments[i], context);
                if (args[i] == null)
                    return null;
            }
            if (call.Subject is IdentifierNode name)
            {
                switch (name.Name)
                {
                    case "round":
                        {
                            if (args.Length < 1 || args.Length > 2)
                            {
                                context.Diagnostics?.Post(new SourceDiagnosticMessage(call.Location, SeverityLevel.Error, "ERR", "Expected 1 or 2 arguments"));
                                return null;
                            }
                            if (args[0] is not double d)
                            {
                                context.Diagnostics?.Post(new SourceDiagnosticMessage(call.Arguments[0].Location, SeverityLevel.Error, "ERR", "Expected number as first argument"));
                                return null;
                            }
                            if (args.Length == 2)
                            {
                                if (args[1] is not double n)
                                {
                                    context.Diagnostics?.Post(new SourceDiagnosticMessage(call.Arguments[1].Location, SeverityLevel.Error, "ERR", "Expected number as second argument"));
                                    return null;
                                }
                                return Math.Round(d, (int)Math.Round(n, MidpointRounding.AwayFromZero));
                            }
                            return Math.Round(d, MidpointRounding.AwayFromZero);
                        }

                    default:
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(call.Location, SeverityLevel.Error, "ERR", "Unrecognized function '{0}'".FormatString(name.Name)));
                        return null;
                }
            }
            throw new NotImplementedException();
        }
        private static object Evaluate(IdentifierNode identifier, EvaluationContext context)
        {
            // If we are actually in the middle of evaluating the identifier, raise an error
            if (context.UsedExpressionParameters.Contains(identifier.Name))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(identifier.Token.Location, SeverityLevel.Error, "ERR", $"Circular variable reference for '{identifier.Name}'"));
                return null;
            }

            // If the current scope contains the evaluated parameter value, use it
            if (context.CurrentScope.TryGetValue(identifier.Name, out object result))
                return result;

            // If not, it might not be evaluated yet, so let's do that now
            if (context.LocalParameterValues.TryGetValue(identifier.Name, out var value))
            {
                context.UsedExpressionParameters.Add(identifier.Name);
                result = context.CurrentScope[identifier.Name] = StatementEvaluator.EvaluateExpression(value, context);
                context.UsedExpressionParameters.Remove(identifier.Name);
                return result;
            }

            // If it is not defined anywhere locally, try to find it back in one of the parent scopes
            var scope = context.CurrentScope.ParentScope;
            while (scope is not null)
            {
                if (scope.TryGetValue(identifier.Name, out result))
                    return result;
                scope = scope.ParentScope;
            }

            // It was not in the current scope, not in one of the parameter to be evaluated, and it wasn't in a parent scope
            // Give up now
            context.Diagnostics?.Post(new SourceDiagnosticMessage(identifier.Location, SeverityLevel.Error, "ERR", $"Could not find variable '{identifier.Name}'"));
            return null;
        }
        private static object Evaluate(NumberNode number, EvaluationContext context)
            => number.Value;
        private static object Evaluate(QuotedNode quoted, EvaluationContext context)
            => quoted.Value.ToString();
        private static object Evaluate(TernaryNode ternary, EvaluationContext context)
        {
            object condition = Evaluate(ternary.Left, context);
            if (condition is null)
                return null;
            if (condition is bool b)
                return b ? Evaluate(ternary.Middle, context) : Evaluate(ternary.Right, context);
            else if (condition is double d)
                return !d.IsZero() ? Evaluate(ternary.Middle, context) : Evaluate(ternary.Right, context);
            else
                return !string.IsNullOrWhiteSpace(condition.ToString()) ? Evaluate(ternary.Middle, context) : Evaluate(ternary.Right, context);
        }
        private static object Evaluate(UnaryNode unary, EvaluationContext context)
        {
            object arg = Evaluate(unary.Argument, context);
            switch (unary.Type)
            {
                case UnaryOperatorTypes.Positive:
                    {
                        if (arg is double d)
                            return d;
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(unary.Location, SeverityLevel.Error, "ERR", "Cannot take positive of non-numbers"));
                        return null;
                    }

                case UnaryOperatorTypes.Negative:
                    {
                        if (arg is double d)
                            return -d;
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(unary.Location, SeverityLevel.Error, "ERR", "Cannot take negative of non-numbers"));
                        return null;
                    }

                case UnaryOperatorTypes.Invert:
                    {
                        if (arg is double d)
                            return d.Equals(0.0) ? 1.0 : 0.0;
                        context.Diagnostics?.Post(new SourceDiagnosticMessage(unary.Location, SeverityLevel.Error, "ERR", "Cannot invert non-numbers"));
                        return null;
                    }

                default:
                    throw new NotImplementedException();
            }
        }
        private static string GetString(this double value)
            => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
