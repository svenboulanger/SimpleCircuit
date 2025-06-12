using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.Nodes;
using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Evaluator
{
    /// <summary>
    /// Evaluator for expressions.
    /// </summary>
    public static class ExpressionEvaluator
    {
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _addition = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => (double)a + (double)b },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => (int)a + (double)b },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => (double)a + (int)b },
            { Tuple.Create(typeof(double), typeof(string)), (a, b) => GetString((double)a) + (string)b },
            { Tuple.Create(typeof(string), typeof(double)), (a, b) => (string)a + GetString((double)b) },
            { Tuple.Create(typeof(int), typeof(string)), (a, b) => a.ToString() + (string)b },
            { Tuple.Create(typeof(string), typeof(int)), (a, b) => a + (string)b },
            { Tuple.Create(typeof(string), typeof(string)), (a, b) => (string)a + (string)b },
            { Tuple.Create(typeof(Vector2), typeof(Vector2)), (a, b) => (Vector2)a + (Vector2)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _subtraction = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => (double)a - (double)b },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => (int)a - (double)b },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => (double)a - (int)b },
            { Tuple.Create(typeof(Vector2), typeof(Vector2)), (a, b) => (Vector2)a - (Vector2)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _multiplication = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => (double)a * (double)b },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => (int)a * (double)b },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => (double)a * (int)b },
            { Tuple.Create(typeof(Vector2), typeof(double)), (a, b) => (Vector2)a * (double)b },
            { Tuple.Create(typeof(double), typeof(Vector2)), (a, b) => (double)a * (Vector2)b },
            { Tuple.Create(typeof(Vector2), typeof(int)), (a, b) => (Vector2)a * (int)b },
            { Tuple.Create(typeof(int), typeof(Vector2)), (a, b) => (int)a * (Vector2)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _division = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => (double)a / (double)b },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => (int)a / (double)b },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => (double)a / (int)b },
            { Tuple.Create(typeof(Vector2), typeof(double)), (a, b) => (Vector2)a / (double)b },
            { Tuple.Create(typeof(Vector2), typeof(int)), (a, b) => (Vector2)a / (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _modulo = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => (double)a % (double)b },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => (int)a % (double)b },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => (double)a % (int)b }
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _logicalAnd = new()
        {
            { Tuple.Create(typeof(bool), typeof(bool)), (a, b) => (bool)a && (bool)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _logicalOr = new()
        {
            { Tuple.Create(typeof(bool), typeof(bool)), (a, b) => (bool)a || (bool)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _and = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)(l & r);
            } },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                return (double)(l & (int)b);
            } },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) =>
            {
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)((int)a & r);
            } },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => (int)a & (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _or = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)(l | r);
            } },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                return (double)(l | (int)b);
            } },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) =>
            {
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)((int)a | r);
            } },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => (int)a | (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _xor = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)(l ^ r);
            } },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                return (double)(l ^ (int)b);
            } },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) =>
            {
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)((int)a ^ r);
            } },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => (int)a ^ (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _equals = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => ((double)a).Equals((double)b) },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => ((double)b).Equals((double)(int)a) },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => ((double)a).Equals((double)(int)b) },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => ((int)a).Equals((int)b) },
            { Tuple.Create(typeof(bool), typeof(bool)), (a, b) => (bool)a == (bool)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _notEquals = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => !((double)a).Equals((double)b) },            
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => !((double)b).Equals((double)(int)a) },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => !((double)a).Equals((double)(int)b) },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => !((int)a).Equals((int)b) },
            { Tuple.Create(typeof(bool), typeof(bool)), (a, b) => (bool)a != (bool)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _smallerThan = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => ((double)a) < ((double)b) },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => (double)a < (int)b },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => (int)a < (double)b },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => (int)a < (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _greaterThan = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => ((double)a) > ((double)b) },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => (double)a > (int)b },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => (int)a > (double)b },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => (int)a > (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _smallerThanOrEqual = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => ((double)a) <= ((double)b) },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => (double)a <= (int)b },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => (int)a <= (double)b },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => (int)a <= (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _greaterThanOrEqual = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) => ((double)a) >= ((double)b) },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) => (double)a >= (int)b },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) => (int)a >= (double)b },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => (int)a >= (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _shiftLeft = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)(l << r);
            } },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                return (double)(l << (int)b);
            } },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) =>
            {
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)((int)a << r);
            } },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => (int)a << (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _shiftRight = new()
        {
            { Tuple.Create(typeof(double), typeof(double)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)(l >> r);
            } },
            { Tuple.Create(typeof(double), typeof(int)), (a, b) =>
            {
                int l = (int)Math.Round((double)a, MidpointRounding.AwayFromZero);
                return (double)(l >> (int)b);
            } },
            { Tuple.Create(typeof(int), typeof(double)), (a, b) =>
            {
                int r = (int)Math.Round((double)b, MidpointRounding.AwayFromZero);
                return (double)((int)a >> r);
            } },
            { Tuple.Create(typeof(int), typeof(int)), (a, b) => (int)a >> (int)b },
        };
        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object, object>> _concatenate = new()
        {
            { Tuple.Create(typeof(string), typeof(string)), (a, b) => (string)a + (string)b }
        };

        private static readonly Dictionary<Tuple<Type>, Func<object, object>> _minus = new()
        {
            { Tuple.Create(typeof(double)), a => -(double)a },
            { Tuple.Create(typeof(int)), a => -(int)a },
            { Tuple.Create(typeof(Vector2)), a => -(Vector2)a },
        };
        private static readonly Dictionary<Tuple<Type>, Func<object, object>> _plus = new()
        {
            { Tuple.Create(typeof(double)), a => +(double)a },
            { Tuple.Create(typeof(int)), a => +(int)a },
            { Tuple.Create(typeof(Vector2)), a => (Vector2)a },
        };
        private static readonly Dictionary<Tuple<Type>, Func<object, object>> _invert = new()
        {
            { Tuple.Create(typeof(bool)), a => !(bool)a },
        };

        /// <summary>
        /// Evaluates a syntax node as an expression.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="context">The context.</param>
        /// <returns>Returns the evaluated value.</returns>
        /// <exception cref="NotImplementedException">Thrown if the node could not be parsed as an expression.</exception>
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
                VectorNode vector => Evaluate(vector, context),
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

            var (name, dict) = binary.Type switch
            {
                BinaryOperatorTypes.Addition => ("add", _addition),
                BinaryOperatorTypes.Subtraction => ("subtract", _subtraction),
                BinaryOperatorTypes.Multiplication => ("multiply", _multiplication),
                BinaryOperatorTypes.Division => ("divide", _division),
                BinaryOperatorTypes.Modulo => ("modulo", _modulo),
                BinaryOperatorTypes.LogicalAnd => ("and", _logicalAnd),
                BinaryOperatorTypes.LogicalOr => ("or", _logicalOr),
                BinaryOperatorTypes.And => ("bitwise and", _and),
                BinaryOperatorTypes.Or => ("bitwise or", _or),
                BinaryOperatorTypes.Xor => ("bitwise xor", _xor),
                BinaryOperatorTypes.Equals => ("compare", _equals),
                BinaryOperatorTypes.NotEquals => ("compare", _notEquals),
                BinaryOperatorTypes.SmallerThan => ("compare", _smallerThan),
                BinaryOperatorTypes.GreaterThan => ("compare", _greaterThan),
                BinaryOperatorTypes.SmallerThanOrEqual => ("compare", _smallerThanOrEqual),
                BinaryOperatorTypes.GreaterThanOrEqual => ("compare", _greaterThanOrEqual),
                BinaryOperatorTypes.ShiftLeft => ("bitshift", _shiftLeft),
                BinaryOperatorTypes.ShiftRight => ("bitshift", _shiftRight),
                BinaryOperatorTypes.Concatenate => ("concatenate", _concatenate),
                _ => throw new NotImplementedException()
            };

            if (dict.TryGetValue(Tuple.Create(left.GetType(), right.GetType()), out var func))
                return func(left, right);
            context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Operator, SeverityLevel.Error, "ERR", $"Cannot {name} types '{left.GetType().Name}' and '{right.GetType().Name}'"));
            return false;
        }
        private static object Evaluate(CallNode call, EvaluationContext context)
        {
            // Build the argument list
            object[] args;
            if (call.Arguments is VectorNode argumentList)
            {
                args = new object[argumentList.Arguments.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = Evaluate(argumentList.Arguments[i], context);
                    if (args[i] == null)
                        return null;
                }
            }
            else
                args = [];

            // Call
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
                                context.Diagnostics?.Post(new SourceDiagnosticMessage(call.Location, SeverityLevel.Error, "ERR", "Expected number as first argument"));
                                return null;
                            }
                            if (args.Length == 2)
                            {
                                if (args[1] is not double n)
                                {
                                    context.Diagnostics?.Post(new SourceDiagnosticMessage(call.Location, SeverityLevel.Error, "ERR", "Expected number as second argument"));
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
            if (arg is null)
                return null;

            var (name, dict) = unary.Type switch
            {
                UnaryOperatorTypes.Positive => ("plus", _plus),
                UnaryOperatorTypes.Negative => ("negate", _minus),
                UnaryOperatorTypes.Invert => ("invert", _invert),
                _ => throw new NotImplementedException()
            };

            if (dict.TryGetValue(Tuple.Create(arg.GetType()), out var func))
                return func(arg);
            context.Diagnostics?.Post(new SourceDiagnosticMessage(unary.Operator, SeverityLevel.Error, "ERR", $"Cannot {name} type '{arg.GetType().Name}'"));
            return false;
        }
        private static object Evaluate(VectorNode vector, EvaluationContext context)
        {
            if (vector.Arguments.Length == 0)
                throw new NotImplementedException();
            if (vector.Arguments.Length == 1)
                return Evaluate(vector.Arguments[0], context);
            if (vector.Arguments.Length == 2)
            {
                // Go for a vector
                if (TryGetDouble(vector.Arguments[0], context, out double x) &&
                    TryGetDouble(vector.Arguments[1], context, out double y))
                    return new Vector2(x, y);
                return null;
            }
            if (vector.Arguments.Length == 4)
            {
                if (TryGetDouble(vector.Arguments[0], context, out double left) &&
                    TryGetDouble(vector.Arguments[1], context, out double top) &&
                    TryGetDouble(vector.Arguments[2], context, out double right) &&
                    TryGetDouble(vector.Arguments[3], context, out double bottom))
                    return new Margins(left, top, right, bottom);
                return null;
            }

            context.Diagnostics?.Post(new SourceDiagnosticMessage(vector.Location, SeverityLevel.Error, "ERR", $"Could not interpret {vector.Arguments.Length} arguments"));
            return null;
        }
        private static string GetString(this double value)
            => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        private static bool TryGetDouble(SyntaxNode node, EvaluationContext context, out double result)
        {
            var arg = Evaluate(node, context);

            if (arg is null)
            {
                result = double.NaN;
                return false;
            }

            if (arg is double d)
            {
                result = d;
                return true;
            }

            if (arg is int i)
            {
                result = i;
                return true;
            }

            result = double.NaN;
            context.Diagnostics?.Post(new SourceDiagnosticMessage(node.Location, SeverityLevel.Error, "ERR", "Expected a number"));
            return false;
        }
    }
}
