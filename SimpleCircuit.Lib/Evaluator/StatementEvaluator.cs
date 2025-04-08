using SimpleCircuit.Components;
using SimpleCircuit.Components.Constraints;
using SimpleCircuit.Components.General;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Evaluator
{
    /// <summary>
    /// An evaluator for evaluating statements.
    /// </summary>
    public static class StatementEvaluator
    {
        private const double _isqrt2 = 0.70710678118;

        /// <summary>
        /// Evaluates a list of statements.
        /// </summary>
        /// <param name="statements">The statements.</param>
        /// <param name="context">The context.</param>
        public static void Evaluate(IEnumerable<SyntaxNode> statements, EvaluationContext context)
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
                case VirtualChainNode virtualChain: Evaluate(virtualChain, context); break;
                case ControlPropertyNode controlProperty: Evaluate(controlProperty, context); break;
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
                    case PinNamePinNode pnp:
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

                    case LiteralNode:
                    case BinaryNode:
                        {
                            // Assume a component name
                            var component = ProcessComponent(item, context);
                            if (component is Drawable drawable)
                            {
                                if (lastWire is not null)
                                    ProcessWire(lastPin, lastWire, new PinReference(drawable, default, item.Location), context);
                                lastWire = null;
                                lastPin = new PinReference(drawable, default, item.Location);
                            }
                            else
                                lastPin = null;
                            lastWire = null;
                        }
                        break;

                    case PropertyListNode property:
                        {
                            // Deal with the component
                            var component = ProcessComponent(property.Subject, context);
                            if (component is Drawable drawable)
                            {
                                if (lastWire is not null)
                                    ProcessWire(lastPin, lastWire, new PinReference(drawable, default, property.Subject.Location), context);
                                lastWire = null;
                                lastPin = new PinReference(drawable, default, property.Subject.Location);

                                // Deal with the properties and variants
                                ApplyPropertiesAndVariants(drawable, property.Properties, context);
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
        private static void Evaluate(VirtualChainNode virtualChain, EvaluationContext context)
        {
            WireNode lastWire = null;
            ILocatedPresence lastPoint = null;
            foreach (var item in virtualChain.Items)
            {
                switch (item)
                {
                    case PinNamePinNode pnp:
                        {
                            // If both pins are specified, let's align on both
                            if (pnp.PinLeft is not null && pnp.PinRight is not null)
                            {
                                // Align on both pins (this might lead to errors if the pins are not exactly aligned properly)
                                var (left, right) = ProcessVirtualComponent(pnp.Name, pnp.PinLeft, pnp.PinRight, virtualChain.Flags, context);
                                if (left is null || right is null)
                                    return;
                                if (lastWire is not null && lastPoint is not null)
                                    ProcessVirtualWire(lastPoint, lastWire, left, virtualChain.Flags, context);
                                lastPoint = right;
                            }
                            else if (pnp.PinLeft is not null)
                            {
                                // Align on left pin
                                var left = ProcessVirtualComponent(pnp.Name, pnp.PinLeft, virtualChain.Flags, context);
                                if (lastWire is not null && lastPoint is not null)
                                    ProcessVirtualWire(lastPoint, lastWire, left, virtualChain.Flags, context);
                                lastPoint = left;
                            }
                            else if (pnp.PinRight is not null)
                            {
                                // Align on right pin
                                var right = ProcessVirtualComponent(pnp.Name, pnp.PinRight, virtualChain.Flags, context);
                                if (lastWire is not null && lastPoint is not null)
                                    ProcessVirtualWire(lastPoint, lastWire, right, virtualChain.Flags, context);
                                lastPoint = right;
                            }
                            else
                            {
                                // Align components
                                var point = ProcessVirtualComponent(pnp.Name, virtualChain.Flags, context);
                                if (lastWire is not null && lastPoint is not null)
                                    ProcessVirtualWire(lastPoint, lastWire, point, virtualChain.Flags, context);
                                lastPoint = point;
                            }
                            lastWire = null;
                        }
                        break;

                    case WireNode wireNode: lastWire = wireNode; break;

                    case LiteralNode:
                    case BinaryNode:
                        {
                            var point = ProcessVirtualComponent(item, virtualChain.Flags, context);
                            if (lastWire is not null && lastPoint is not null)
                                ProcessVirtualWire(lastPoint, lastWire, point, virtualChain.Flags, context);
                            lastWire = null;
                            lastPoint = point;
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
        private static void Evaluate(ControlPropertyNode controlProperty, EvaluationContext context)
        {
            string key = controlProperty.Key.Content.ToString();

            // Check whether the key is actually a key
            if (!context.Factory.IsKey(key))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(controlProperty.Key, SeverityLevel.Error, "ERR", "Expected component key"));
                return;
            }

            StorePropertiesAndVariants(key, context.Options, controlProperty.Properties, context);
        }

        private static void ApplyPropertiesAndVariants(IDrawable presence, IEnumerable<SyntaxNode> properties, EvaluationContext context)
        {
            int labelIndex = 0;
            foreach (var property in properties)
            {
                switch (property)
                {
                    case UnaryNode unary:
                        if (unary.Type == UnaryOperatorTypes.Positive)
                        {
                            // Add a variant
                            string name = EvaluateName(unary.Argument, context);
                            presence.Variants.Add(name);
                        }
                        else if (unary.Type == UnaryOperatorTypes.Negative)
                        {
                            // Remove a variant
                            string name = EvaluateName(unary.Argument, context);
                            presence.Variants.Remove(name);
                        }
                        else
                            throw new NotImplementedException();
                        break;

                    case QuotedNode quoted:
                        {
                            // Add a label
                            presence.Labels[labelIndex].Value = quoted.Value.ToString();
                            labelIndex++;
                        }
                        break;

                    case IdentifierNode id:
                        {
                            // Add a variant
                            presence.Variants.Add(id.Name);
                        }
                        break;

                    case BinaryNode binary:
                        switch (binary.Type)
                        {
                            case BinaryOperatorTypes.Concatenate:
                                break;

                            case BinaryOperatorTypes.Assignment:
                                // Set property
                                if (binary.Left is not IdentifierNode id)
                                {
                                    context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Location, SeverityLevel.Error, "ERR", "Expected a literal"));
                                    return;
                                }
                                string propertyName = id.Name;
                                object value = EvaluateExpression(binary.Right, context);
                                if (!presence.SetProperty(id.Token, value, context.Diagnostics))
                                    return;
                                break;
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
        private static void StorePropertiesAndVariants(string key, Options options, IEnumerable<SyntaxNode> properties, EvaluationContext context)
        {
            int labelIndex = 0;
            foreach (var property in properties)
            {
                switch (property)
                {
                    case UnaryNode unary:
                        if (unary.Type == UnaryOperatorTypes.Positive)
                        {
                            // Add a variant
                            string name = EvaluateName(unary.Argument, context);
                            options.AddInclude(key, name);
                        }
                        else if (unary.Type == UnaryOperatorTypes.Negative)
                        {
                            // Remove a variant
                            string name = EvaluateName(unary.Argument, context);
                            options.AddExclude(key, name);
                        }
                        else
                            throw new NotImplementedException();
                        break;

                    case QuotedNode quoted:
                        {
                            // Add a label
                            options.AddDefaultProperty(key, new Token(quoted.Location, $"label{labelIndex}".AsMemory()), quoted.Value.ToString());
                            labelIndex++;
                        }
                        break;

                    case IdentifierNode id:
                        {
                            // Add a variant
                            options.AddInclude(key, id.Name);
                        }
                        break;

                    case BinaryNode binary:
                        switch (binary.Type)
                        {
                            case BinaryOperatorTypes.Concatenate:
                                break;

                            case BinaryOperatorTypes.Assignment:
                                // Set property
                                if (binary.Left is not IdentifierNode id)
                                {
                                    context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Location, SeverityLevel.Error, "ERR", "Expected a literal"));
                                    return;
                                }
                                string propertyName = id.Name;
                                object value = EvaluateExpression(binary.Right, context);
                                options.AddDefaultProperty(key, id.Token, value);
                                break;
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
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
                            if (direction.Distance is UnaryNode unary && unary.Type == UnaryOperatorTypes.Positive)
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
        private static void ProcessVirtualWire(ILocatedPresence start, WireNode wire, ILocatedPresence end, VirtualChainConstraints flags, EvaluationContext context)
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
                            if (direction.Distance is MinimumNode minimum)
                                segment.Length = EvaluateAsNumber(minimum.Distance, context, context.Options.MinimumWireLength);
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

            // Create the virtual wire
            if (segments.Count > 0)
            {
                var result = new VirtualWire(context.GetVirtualName(), start, segments, end, flags);
                context.Circuit.Add(result);
            }
        }
        private static ICircuitPresence ProcessComponent(SyntaxNode component, EvaluationContext context)
        {
            // Evaluate the full name
            string name = EvaluateName(component, context);
            if (name is null)
                return null;

            // Validate the name of the component
            if (!IsValidName(name))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(component.Location, SeverityLevel.Error, "ERR", $"Invalid name '{name}'"));
                return null;
            }

            // Expand the name
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
        private static ILocatedPresence ProcessVirtualComponent(SyntaxNode component, VirtualChainConstraints flags, EvaluationContext context)
        {
            // Evaluate and validate the name
            string name = EvaluateVirtualName(component, context);
            if (name is null)
                return null;

            // Return the result
            var result = new AlignedComponents(context.GetVirtualName(), name, flags);
            context.Circuit.Add(result);
            return result;
        }
        private static ILocatedPresence ProcessVirtualComponent(SyntaxNode component, SyntaxNode pin, VirtualChainConstraints flags, EvaluationContext context)
        {
            // Evaluate and validate the name
            string name = EvaluateVirtualName(component, context);
            if (name is null)
                return null;

            // Evaluate the pin name
            string pinName = EvaluateName(pin, context);
            if (pinName is null)
                return null;

            // Return the result
            var result = new AlignedPins(context.GetVirtualName(), name, pinName, flags);
            context.Circuit.Add(result);
            return result;
        }
        private static (ILocatedPresence, ILocatedPresence) ProcessVirtualComponent(SyntaxNode component, SyntaxNode left, SyntaxNode right, VirtualChainConstraints flags, EvaluationContext context)
        {
            // Evaluate and validate the name
            string name = EvaluateVirtualName(component, context);
            if (name is null)
                return (null, null);

            // Evaluate the pin names
            string pinLeft = EvaluateName(left, context);
            string pinRight = EvaluateName(right, context);
            if (pinLeft is null || pinRight is null)
                return (null, null);

            // Return the result
            var resultLeft = new AlignedPins(context.GetVirtualName(), name, pinLeft, flags);
            var resultRight = new AlignedPins(context.GetVirtualName(), name, pinRight, flags);
            return (resultLeft, resultRight);
        }
        private static string EvaluateName(SyntaxNode node, EvaluationContext context)
        {
            switch (node)
            {
                case LiteralNode literalNode:
                    return literalNode.Value.ToString();

                case BinaryNode binaryNode when binaryNode.Type == BinaryOperatorTypes.Concatenate:
                    string left = EvaluateName(binaryNode.Left, context);
                    string right = EvaluateName(binaryNode.Right, context);
                    return left + right;

                case BracketNode bracketNode when bracketNode.Left.Content.ToString() == "{" && bracketNode.Right.Content.ToString() == "}":
                    return ExpressionEvaluator.Evaluate(bracketNode.Value, context)?.ToString();

                default:
                    context.Diagnostics?.Post(new SourceDiagnosticMessage(node.Location, SeverityLevel.Error, "ERR", "Unable to resolve name"));
                    return null;
            }
        }
        private static string EvaluateVirtualName(SyntaxNode node, EvaluationContext context)
        {
            // Evaluate and validate the name
            string name = EvaluateName(node, context);
            if (name is null)
                return null;
            if (!IsValidName(name, true))
            {
                context.Diagnostics?.Post(new SourceDiagnosticMessage(node.Location, SeverityLevel.Error, "ERR", $"The virtual component filter {name} is invalid"));
                return null;
            }

            // Resolve the name as a filter
            name = context.GetFullname(name, resolveAnonymous: false).Replace("*", ".*");
            if (context.Factory.IsAnonymous(name))
                name = $"^{name}{DrawableFactoryDictionary.AnonymousSeparator}.*$";
            else
                name = $"^{name}$";
            return name;
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
        private static bool IsValidName(string name, bool allowWildcards = false)
        {
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsLetterOrDigit(c) || c == '_')
                    continue;
                if (c == DrawableFactoryDictionary.Separator)
                    continue;
                if (c == '*')
                {
                    if (allowWildcards)
                        continue;
                    return false;
                }
            }
            return true;
        }
        private static object EvaluateExpression(SyntaxNode expression, EvaluationContext context)
        {
            switch (expression)
            {
                case BracketNode bracket when bracket.Left.Content.Span[0] == '{':
                    return ExpressionEvaluator.Evaluate(bracket.Value, context);

                default:
                    return ExpressionEvaluator.Evaluate(expression, context);
            }
        }
    }
}
