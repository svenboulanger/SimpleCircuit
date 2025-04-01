using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components;
using SimpleCircuit.Components.Constraints;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using SimpleCircuit.Parser.SimpleTexts;
using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Evaluator
{
    /// <summary>
    /// A context for evaluating the AST for SimpleCircuit.
    /// </summary>
    public class EvaluationContext
    {
        private readonly Stack<string> _sections = new();
        private readonly Dictionary<string, int> _anonymousCounters = [];

        /// <summary>
        /// Gets the options.
        /// </summary>
        public Options Options { get; } = new();

        /// <summary>
        /// Gets or sets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public Dictionary<string, SyntaxNode> Parameters { get; set; }

        /// <summary>
        /// Gets the factory for components.
        /// </summary>
        public DrawableFactoryDictionary Factory { get; } = new();

        /// <summary>
        /// Gets the circuit.
        /// </summary>
        public GraphicalCircuit Circuit { get; }

        /// <summary>
        /// Create a new parsing context with the default stuff in it.
        /// </summary>
        /// <param name="loadAssembly">If <c>true</c>, the assembly should be searched for components using reflection.</param>
        /// <param name="formatter">The text formatter used for the graphical circuit.</param>
        public EvaluationContext(bool loadAssembly = true, ITextFormatter formatter = null)
        {
            if (loadAssembly)
                Factory.RegisterAssembly(typeof(ParsingContext).Assembly);
            Circuit = new GraphicalCircuit(formatter ?? new SimpleTextFormatter(new SkiaTextMeasurer()));
        }

        /// <summary>
        /// Gets the full name based on the current section stack.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The full name.</returns>
        public string GetFullname(string name)
        {
            if (Factory.IsAnonymous(name, out string key))
            {
                if (_anonymousCounters.TryGetValue(key, out int counter))
                {
                    name = $"{name}{DrawableFactoryDictionary.AnonymousSeparator}{counter}";
                    _anonymousCounters[key] = counter + 1;
                }
                else
                {
                    name = $"{name}{DrawableFactoryDictionary.AnonymousSeparator}1";
                    _anonymousCounters[key] = 2;
                }

            }
            return string.Join(DrawableFactoryDictionary.Separator, _sections.Reverse().Union([name]));
        }

        /// <summary>
        /// Gets the full name for an anonymous point.
        /// </summary>
        /// <returns>Gets the full name of the anonymous point.</returns>
        public string GetAnonymousPointName()
        {
            string name;
            if (_anonymousCounters.TryGetValue(PointFactory.Key, out int counter))
            {
                name = $"{PointFactory.Key}{DrawableFactoryDictionary.AnonymousSeparator}{counter}";
                _anonymousCounters[PointFactory.Key] = counter + 1;
            }
            else
            {
                name = $"{PointFactory.Key}{DrawableFactoryDictionary.AnonymousSeparator}1";
                _anonymousCounters[PointFactory.Key] = 2;
            }
            return string.Join(DrawableFactoryDictionary.Separator, _sections.Reverse().Union([name]));
        }

        /// <summary>
        /// Gets a wire name.
        /// </summary>
        /// <returns>The wire name.</returns>
        public string GetWireName()
        {
            string name;
            if (_anonymousCounters.TryGetValue(":wire:", out int counter))
            {
                name = $"wire{counter}";
                _anonymousCounters[":wire:"] = counter + 1;
            }
            else
            {
                name = "wire-1";
                _anonymousCounters[":wire:"] = 2;
            }
            return string.Join(DrawableFactoryDictionary.Separator, _sections.Reverse().Union([name]));
        }
    }

    public static class NodeEvaluator
    {
        private const double _isqrt2 = 0.70710678118;

        public static void Evaluate(List<SyntaxNode> statements, EvaluationContext context)
        {
            // First evaluate all the .param statements

            // Then handle all the statements as they go
            foreach (var statement in statements)
                Evaluate(statement, context);
        }

        private static void Evaluate(SyntaxNode statement, EvaluationContext context)
        {
            switch (statement)
            {
                case ComponentChain chain: Evaluate(chain, context); break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void Evaluate(ComponentChain chain, EvaluationContext context)
        {
            WireNode lastWire = null;
            PinReference lastPin = null;
            foreach (var item in chain.Items)
            {
                switch (item)
                {
                    case PinNamePin pnp:
                        {
                            // Assume a component
                            var component = ProcessComponent(pnp.Name, context);
                            if (component is Drawable drawable)
                            {
                                if (pnp.PinLeft is not null)
                                {
                                    string name = EvaluateName(pnp.PinLeft, context);
                                    if (name is not null && lastWire is not null)
                                        ProcessWire(lastPin, lastWire, new PinReference(drawable, name, pnp.PinLeft.Location), context);
                                }
                                else if (lastWire is not null)
                                    ProcessWire(lastPin, lastWire, new PinReference(drawable, null, pnp.Name.Location), context);
                                lastWire = null;

                                if (pnp.PinRight is not null)
                                {
                                    string name = EvaluateName(pnp.PinRight, context);
                                    if (name is not null)
                                        lastPin = new PinReference(drawable, name, pnp.PinRight.Location);
                                }
                                else
                                    lastPin = new PinReference(drawable, null, pnp.Name.Location);
                            }
                            else
                                lastPin = null;
                        }
                        break;

                    case WireNode w: lastWire = w; break;

                    case Literal:
                    case BinaryNode:
                        {
                            // Assume a component name
                            var component = ProcessComponent(item, context);
                            if (component is Drawable drawable)
                            {
                                if (lastWire is not null)
                                    ProcessWire(lastPin, lastWire, new PinReference(drawable, default, item.Location), context);
                                lastPin = new PinReference(drawable, default, item.Location);
                            }
                            else
                                lastPin = null;
                            lastWire = null;
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            if (lastWire is not null)
                ProcessWire(lastPin, lastWire, null, context);
        }

        private static void ProcessWire(PinReference startPin, WireNode wire, PinReference endPin, EvaluationContext context)
        {
            List<WireSegmentInfo> segments = [];
            foreach (var item in wire.Items)
            {
                switch (item)
                {
                    case DirectionNode direction:
                        // Create the segment and apply defaults
                        var segment = new WireSegmentInfo(direction.Location)
                        {
                            IsMinimum = true,
                            Length = context.Options.MinimumWireLength
                        };

                        if (direction.Angle is not null)
                        {
                            double angle = EvaluateAsNumber(direction.Angle, context, 0.0);
                            segment.Orientation = Vector2.Normal(angle / 180.0 * Math.PI);
                        }
                        else
                        {
                            // Use the direction name itself
                            segment.Orientation = direction.Value.ToString() switch
                            {
                                DirectionNode.UpArrow or "n" or "u" => new(0, -1),
                                DirectionNode.DownArrow or "s" or "d" => new(0, 1),
                                DirectionNode.LeftArrow or "e" or "l" => new(-1, 0),
                                DirectionNode.RightArrow or "w" or "r" => new(1, 0),
                                DirectionNode.UpLeftArrow or "nw" => new(-_isqrt2, -_isqrt2),
                                DirectionNode.UpRightArrow or "ne" => new(_isqrt2, -_isqrt2),
                                DirectionNode.DownRightArrow or "se" => new(_isqrt2, _isqrt2),
                                DirectionNode.DownLeftArrow or "sw" => new(-_isqrt2, _isqrt2),
                                _ => throw new NotImplementedException(),
                            };
                        }
                        if (direction.Distance is not null)
                        {
                            if (direction.Distance is Unary unary && unary.Type == UnaryOperatorTypes.Positive)
                                segment.Length = EvaluateAsNumber(unary.Argument, context, context.Options.MinimumWireLength);
                            else
                            {
                                segment.Length = EvaluateAsNumber(direction.Distance, context, context.Options.MinimumWireLength);
                                segment.IsMinimum = false;
                            }
                        }
                        segments.Add(segment);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            // Create pin constraints
            string wireName = context.GetWireName();
            if (segments.Count > 0)
            {
                // Create anonymous points if the pin isn't specified
                if (startPin is null)
                {
                    var point = context.Factory.Create(context.GetAnonymousPointName(), context.Options, context.Diagnostics);
                    context.Circuit.Add(point);
                    startPin = new PinReference(point, null, default);
                }
                if (endPin is null)
                {
                    var point = context.Factory.Create(context.GetAnonymousPointName(), context.Options, context.Diagnostics);
                    context.Circuit.Add(point);
                    endPin = new PinReference(point, null, default);
                }
                context.Circuit.Add(new PinOrientationConstraint($"{wireName}.s", startPin, -1, segments[0], false));
                context.Circuit.Add(new PinOrientationConstraint($"{wireName}.e", endPin, 0, segments[^1], true));

                // Create the wire
                var result = new Wire(wireName, startPin, segments, endPin);
                context.Circuit.Add(result);
            }
        }

        private static ICircuitPresence ProcessComponent(SyntaxNode component, EvaluationContext context)
        {
            // Evaluate the full name
            string name = EvaluateName(component, context);
            if (name is null)
                return null;
            name = context.GetFullname(name);

            // Get the item
            if (!context.Circuit.TryGetValue(name, out var presence))
            {
                presence = context.Factory.Create(name, context.Options, context.Diagnostics);
                context.Circuit.Add(presence);
            }

            // Register the source
            presence.Sources.Add(component.Location);
            return presence;
        }

        private static string EvaluateName(SyntaxNode node, EvaluationContext context)
        {
            if (node is Literal literal)
                return literal.Value.ToString();
            if (node is BinaryNode binary && binary.Type == BinaryOperatorTypes.Concatenate)
            {
                string left = EvaluateName(binary.Left, context);
                string right = EvaluateName(binary.Right, context);
                return left + right;
            }
            context.Diagnostics?.Post(new SourceDiagnosticMessage(node.Location, SeverityLevel.Error, "ERR", "Unable to resolve component name"));
            return null;
        }

        private static double EvaluateAsNumber(SyntaxNode node, EvaluationContext context, double defaultValue)
        {
            object result = ExpressionEvaluator.Evaluate(node, context);
            if (result is null)
                return defaultValue;
            if (result is double dResult)
                return dResult;
            else
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(node.Location, SeverityLevel.Error, "ERR", "Cannot evaluate as a number"));
                return defaultValue;
            }
        }
    }

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
                Number number => Evaluate(number, context),
                Quoted quoted => Evaluate(quoted, context),
                Ternary ternary => Evaluate(ternary, context),
                Unary unary => Evaluate(unary, context),
                _ => throw new NotImplementedException(),
            };
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
            if (context.Parameters.TryGetValue(identifier.Name, out var node))
                return Evaluate(node, context);
            context.Diagnostics?.Post(new SourceDiagnosticMessage(identifier.Location, SeverityLevel.Error, "ERR", "Cannot find parameter '{0}'".FormatString(identifier.Name)));
            return null;
        }
        private static object Evaluate(Number number, EvaluationContext context)
            => number.Value;
        private static object Evaluate(Quoted quoted, EvaluationContext context)
            => quoted.Value.ToString();
        private static object Evaluate(Ternary ternary, EvaluationContext context)
        {
            var condition = Evaluate(ternary.Left, context);
            if (condition is null)
                return null;
            if (condition is double d)
            {
                if (d.Equals(0.0))
                    return Evaluate(ternary.Middle, context);
                else
                    return Evaluate(ternary.Right, context);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(condition.ToString()))
                    return Evaluate(ternary.Middle, context);
                else
                    return Evaluate(ternary.Right, context);
            }
        }
        private static object Evaluate(Unary unary, EvaluationContext context)
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
