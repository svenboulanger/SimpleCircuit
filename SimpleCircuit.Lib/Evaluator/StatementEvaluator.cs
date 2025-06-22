using SimpleCircuit.Components;
using SimpleCircuit.Components.Annotations;
using SimpleCircuit.Components.Constraints;
using SimpleCircuit.Components.General;
using SimpleCircuit.Components.Markers;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using SpiceSharp;
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
        public const double MinimumWireLength = -1.0;

        /// <summary>
        /// Evaluates a list of statements.
        /// </summary>
        /// <param name="statement">The node to evaluate.</param>
        /// <param name="context">The context.</param>
        public static void Evaluate(SyntaxNode statement, EvaluationContext context)
        {
            if (statement is null)
                return; // There's nothing to evaluate

            switch (statement)
            {
                case ScopedStatementsNode statements: EvaluateScoped(statements, context); break;
                case ComponentChain chain: Evaluate(chain, context); break;
                case VirtualChainNode virtualChain: Evaluate(virtualChain, context); break;
                case ControlPropertyNode controlProperty: Evaluate(controlProperty, context); break;
                case BoxNode annotation: Evaluate(annotation, context); break;
                case ScopeDefinitionNode scopeDefinition: Evaluate(scopeDefinition, context); break;
                case SectionDefinitionNode sectionDefinition: Evaluate(sectionDefinition, context); break;
                case ForLoopNode forLoop: Evaluate(forLoop, context); break;
                case SymbolDefinitionNode symbolDefinition: Evaluate(symbolDefinition, context); break;
                case IfElseNode ifElse: Evaluate(ifElse, context); break;
                case SubcircuitDefinitionNode subckt: Evaluate(subckt, context); break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void EvaluateScoped(ScopedStatementsNode statements, EvaluationContext context)
        {
            // First set up the new scope, and apply the parameter definitions to them
            context.StartScope();
            Evaluate(statements, context);
            context.EndScope();
        }
        private static void Evaluate(ScopedStatementsNode statements, EvaluationContext context)
        {
            ApplyLocalParameterDefinitions(statements.ParameterDefinitions, statements.DefaultVariantsAndProperties, context);

            // Evaluate the statements
            foreach (var statement in statements.Statements)
                Evaluate(statement, context);
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
                            if (component is IDrawable drawable)
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


                    case LiteralNode:
                    case BinaryNode:
                    case PropertyListNode:
                        {
                            // Assume a component name
                            var component = ProcessComponent(item, context);
                            if (component is IDrawable drawable)
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

                    case QueuedAnonymousPoint qap:
                        {
                            var pointName = context.GetAnonymousPointName();
                            var point = context.Factory.Create(pointName, context.Options, context.CurrentScope, context.Diagnostics) as ILocatedDrawable;
                            context.NotifyDrawable(point);
                            context.Circuit.Add(point);
                            context.QueuedPoints.Enqueue(point);
                            if (lastWire is not null)
                                ProcessWire(lastPin, lastWire, new PinReference(point, default, item.Location), context);
                            lastWire = null;
                            lastPin = new PinReference(point, default, item.Location);
                        }
                        break;

                    case WireNode w: lastWire = w; break;

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
            string filter = EvaluateFilter(controlProperty.Filter, context);
            if (filter is null)
                return;

            int labelIndex = 0;
            HashSet<string> includes = [], excludes = [];
            List<(Token, object)> defaultProperties = [];

            foreach (var property in controlProperty.Properties)
            {
                switch (property)
                {
                    case UnaryNode unary:
                        if (unary.Type == UnaryOperatortype.Positive)
                        {
                            // Add a variant
                            string name = EvaluateName(unary.Argument, context);
                            includes.Add(name);
                        }
                        else if (unary.Type == UnaryOperatortype.Negative)
                        {
                            // Remove a variant
                            string name = EvaluateName(unary.Argument, context);
                            excludes.Add(name);
                        }
                        else
                            throw new NotImplementedException();
                        break;

                    case QuotedNode quoted:
                        {
                            // Add a label
                            defaultProperties.Add((new Token(quoted.Location, $"label{labelIndex}".AsMemory()), quoted.Value.ToString()));
                            labelIndex++;
                        }
                        break;

                    case IdentifierNode id:
                        {
                            // Add a variant
                            includes.Add(id.Name);
                        }
                        break;

                    case LiteralNode literal:
                        {
                            // Add a variant
                            includes.Add(literal.Value.ToString());
                        }
                        break;

                    case BinaryNode binary:
                        switch (binary.Type)
                        {
                            case BinaryOperatortype.Concatenate:
                                break;

                            case BinaryOperatortype.Assignment:
                                // Set property
                                string propertyName = EvaluateName(binary.Left, context);
                                if (propertyName is null)
                                    return;
                                object value = EvaluateExpression(binary.Right, context);
                                if (value is null)
                                    return;

                                defaultProperties.Add((new(binary.Location, propertyName.AsMemory()), value));
                                break;
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            // Add the defaults to the current scope
            context.CurrentScope.AddDefault(filter, includes, excludes, defaultProperties);
        }
        private static void Evaluate(BoxNode annotation, EvaluationContext context)
        {
            context.StartScope();
            var box = new Box(context.GetAnnotationName());
            context.Circuit.Add(box);

            // Apply properties and variants
            ApplyPropertiesAndVariants(box, annotation.Properties, context);

            // Evaluate the scoped statements
            context.StartTrackingDrawables();
            Evaluate(annotation.Statements, context);
            var set = context.StopTrackingDrawables();

            // Add the drawables to the annotation
            foreach (var drawable in set)
                box.Add(drawable);

            context.EndScope();
        }
        private static void Evaluate(ScopeDefinitionNode scopeDefinition, EvaluationContext context)
        {
            context.StartScope();
            foreach (var property in scopeDefinition.Parameters)
            {
                // Try to parse a property and update the local scope
                if (property is not BinaryNode assignment ||
                    assignment.Type != BinaryOperatortype.Assignment)
                {
                    context.Diagnostics?.Post(property.Location, ErrorCodes.ExpectedPropertyAssignment);
                    return;
                }
                if (assignment.Left is not IdentifierNode propertyName)
                {
                    context.Diagnostics?.Post(assignment.Left.Location, ErrorCodes.ExpectedPropertyName);
                    return;
                }

                // Update the value
                object value = EvaluateExpression(assignment.Right, context);
                if (value is null)
                    return;
                context.CurrentScope[propertyName.Name] = value;
            }

            // Execute the statements under a new section
            Evaluate(scopeDefinition.Statements, context);
            context.EndScope();
        }
        private static void Evaluate(SectionDefinitionNode sectionDefinition, EvaluationContext context)
        {
            // First determine the name of the section
            string name = EvaluateName(sectionDefinition.Name, context);
            if (name is null)
                return;

            var template = sectionDefinition;
            if (sectionDefinition.Template is not null)
            {
                string templateName = EvaluateName(sectionDefinition.Template, context);
                if (!context.SectionDefinitions.TryGetValue(templateName, out template))
                {
                    context.Diagnostics?.Post(sectionDefinition.Template.Location, ErrorCodes.CouldNotFindSectionWithName, templateName);
                    return;
                }
            }

            // Setup the right context for evaluating the section
            context.StartSection(name);
            foreach (var property in template.Properties)
            {
                // Try to parse a property and update the local scope
                if (property is not BinaryNode assignment ||
                    assignment.Type != BinaryOperatortype.Assignment)
                {
                    context.Diagnostics?.Post(property.Location, ErrorCodes.ExpectedPropertyAssignment);
                    return;
                }

                // Update the value
                string propertyName = EvaluateName(assignment.Left, context);
                if (propertyName is null)
                    return;
                object value = EvaluateExpression(assignment.Right, context);
                if (value is null)
                    return;
                context.CurrentScope[propertyName] = value;
            }

            // If the template is not the same as the current section, override the default properties from before
            if (!ReferenceEquals(sectionDefinition, template))
            {
                // Ignore the first property since that is supposed to be the name of the template
                foreach (var property in sectionDefinition.Properties)
                {
                    if (property is not BinaryNode assignment || assignment.Type != BinaryOperatortype.Assignment)
                    {
                        context.Diagnostics?.Post(property.Location, ErrorCodes.ExpectedPropertyAssignment);
                        return;
                    }

                    // Update the value
                    string propertyName = EvaluateName(assignment.Left, context);
                    if (propertyName is null)
                        return;
                    object value = EvaluateExpression(assignment.Right, context);
                    if (value is null)
                        return;
                    context.CurrentScope[propertyName] = value;
                }
            }

            // Execute the statements under a new section
            Evaluate(template.Statements, context);
            context.EndSection();

            // Register the section as a template if there was no base template given
            if (ReferenceEquals(sectionDefinition, template))
                context.SectionDefinitions[name] = sectionDefinition;
        }
        private static void Evaluate(ForLoopNode forLoop, EvaluationContext context)
        {
            // Evaluate the values for the for-loop
            string variableName = forLoop.Variable.Content.ToString();
            double start = EvaluateAsDouble(forLoop.Start, context, 0.0);
            double end = EvaluateAsDouble(forLoop.End, context, 1.0);
            double increment = EvaluateAsDouble(forLoop.Increment, context, 1.0);

            // Make sure we don't end up in an infinite loop
            if (increment.IsZero())
            {
                context.Diagnostics?.Post(forLoop.Increment.Location, ErrorCodes.ForLoopIncrementTooSmall, increment);
                return;
            }
            
            // Execute the for-loop
            if (end > start)
            {
                if (increment < 0)
                    increment = -increment;
                double value = start;
                while (value < end + 1e-9)
                {
                    context.StartScope();
                    context.CurrentScope[variableName] = value;
                    Evaluate(forLoop.Statements, context);
                    context.EndScope();
                    value += increment;
                }
            }
            else
            {
                if (increment > 0)
                    increment = -increment;
                double value = start;
                while (value > end - 1e-9)
                {
                    context.StartScope();
                    context.CurrentScope[variableName] = value;
                    Evaluate(forLoop.Statements, context);
                    context.EndScope();
                    value += increment;
                }
            }
        }
        private static void Evaluate(SymbolDefinitionNode symbolDefinition, EvaluationContext context)
        {
            string key = symbolDefinition.Key.Content.ToString();
            context.Factory.Register(new XmlDrawable(key, symbolDefinition.Xml, context.Diagnostics));
        }
        private static void Evaluate(IfElseNode ifElse, EvaluationContext context)
        {
            foreach (var condition in ifElse.Conditions)
            {
                object value = EvaluateExpression(condition.Condition, context);
                switch (value)
                {
                    case double d:
                        if (d.IsZero())
                            continue;
                        break;

                    case int integer:
                        if (integer == 0)
                            continue;
                        break;

                    case bool b:
                        if (!b)
                            continue;
                        break;

                    default:
                        continue;
                }

                // Evaluate the statements of the condition
                EvaluateScoped(condition.Statements, context);
                return;
            }

            // If we reached here, then none of the conditions matched
            if (ifElse.Else is not null)
                EvaluateScoped(ifElse.Else, context);
        }
        private static void Evaluate(SubcircuitDefinitionNode subckt, EvaluationContext context)
        {
            // Create a new factory
            context.Factory.Register(new Subcircuit(subckt.Name.Content.ToString(), subckt, context.Factory, context.Options, context.Diagnostics));
        }

        private static void ApplyLocalParameterDefinitions(IEnumerable<ParameterDefinitionNode> parameterDefinitions, IEnumerable<ControlPropertyNode> defaultVariantsAndProperties, EvaluationContext context)
        {
            // Make sure we can find the parameter definitions back
            foreach (var parameterDefinition in parameterDefinitions)
                context.LocalParameterValues[parameterDefinition.Name] = parameterDefinition.Value;

            // Evaluate all the parameter definitions for the current scope
            foreach (var parameterDefinition in parameterDefinitions)
            {
                // Make sure we avoid evaluating circular expressions
                context.UsedExpressionParameters.Add(parameterDefinition.Name);

                // Evaluate the value of the parameter
                object value = EvaluateExpression(parameterDefinition.Value, context);
                context.CurrentScope[parameterDefinition.Name] = value;

                // Release the used parameter name
                context.UsedExpressionParameters.Remove(parameterDefinition.Name);
            }

            // Then evaluate all default variants and properties
            foreach (var defaultOptions in defaultVariantsAndProperties)
                Evaluate(defaultOptions, context);
        }
        private static void ApplyPropertiesAndVariants(IDrawable presence, IEnumerable<SyntaxNode> properties, EvaluationContext context, HashSet<Marker> markers = null)
        {
            int labelIndex = 0;
            foreach (var property in properties)
            {
                switch (property)
                {
                    case UnaryNode unary:
                        if (unary.Type == UnaryOperatortype.Positive)
                        {
                            // Add a variant
                            string name = EvaluateName(unary.Argument, context);
                            presence.Variants.Add(name);
                        }
                        else if (unary.Type == UnaryOperatortype.Negative)
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

                    case LiteralNode literal:
                        {
                            // Add a variant
                            presence.Variants.Add(literal.Value.ToString());
                        }
                        break;

                    case BinaryNode binary:
                        switch (binary.Type)
                        {
                            case BinaryOperatortype.Concatenate:
                                break;

                            case BinaryOperatortype.Assignment:
                                // Set property
                                string propertyName = EvaluateName(binary.Left, context);
                                if (propertyName is null)
                                    return;
                                object value = EvaluateExpression(binary.Right, context);
                                if (!presence.SetProperty(new(binary.Left.Location, propertyName.AsMemory()), value, context.Diagnostics))
                                    return;
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
            List<Marker> markers = [];
            List<SyntaxNode> propertiesAndVariants = [];
            for (int i = 0; i < wire.Items.Length; i++)
            {
                var item = wire.Items[i];
                switch (item)
                {
                    case DirectionNode direction:
                        {
                            // Create the segment and apply defaults
                            var segment = new WireSegmentInfo(direction.Location)
                            {
                                IsMinimum = true,
                                Length = MinimumWireLength
                            };

                            // Handle markers
                            if (markers.Count > 0)
                            {
                                if (segments.Count == 0)
                                    segment.StartMarkers = [.. markers];
                                else
                                    segments[^1].EndMarkers = [.. markers];
                            }
                            markers.Clear();

                            if (direction.Angle is not null)
                            {
                                double angle = EvaluateAsDouble(direction.Angle, context, 0.0);
                                segment.Orientation = Vector2.Normal(-angle / 180.0 * Math.PI);
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
                                    "?" => new(),
                                    "??" => new(double.NaN, double.NaN),
                                    _ => throw new NotImplementedException(),
                                };
                            }
                            if (direction.Distance is not null)
                            {
                                if (direction.Distance is UnaryNode unary && unary.Type == UnaryOperatortype.Positive)
                                    segment.Length = EvaluateAsDouble(unary.Argument, context, MinimumWireLength);
                                else
                                {
                                    segment.Length = EvaluateAsDouble(direction.Distance, context, MinimumWireLength);
                                    segment.IsMinimum = false;
                                }
                            }
                            segments.Add(segment);
                        }
                        break;

                    case BinaryNode bn when bn.Type == BinaryOperatortype.Assignment:
                        propertiesAndVariants.Add(item);
                        break;

                    case LiteralNode literal:
                        if (literal.Value.Length == 1 && literal.Value.Span[0] == '-')
                        {
                            // Create the segment and apply defaults
                            var segment = new WireSegmentInfo(literal.Location)
                            {
                                IsMinimum = true,
                                Length = MinimumWireLength
                            };
                            segments.Add(segment);
                        }
                        else
                        {
                            string name = literal.Value.ToString();
                            if (context.Markers.TryGetValue(name, out var func))
                                markers.Add(func());
                            else
                                propertiesAndVariants.Add(literal);
                        }
                        break;

                    case UnaryNode unary:
                        if (unary.Type == UnaryOperatortype.Positive)
                        {
                            // Variant - force evaluation immediate
                            string name = EvaluateName(unary.Argument, context);
                            if (name is null)
                                continue;
                            if (context.Markers.TryGetValue(name, out var func))
                                markers.Add(func());
                            else
                                propertiesAndVariants.Add(new IdentifierNode(new(unary.Location, name.AsMemory())));
                        }
                        else if (unary.Type == UnaryOperatortype.Negative)
                            propertiesAndVariants.Add(unary); // Can't "remove" markers - so we already know it's a variant
                        else
                            throw new NotImplementedException();
                        break;

                    case IdentifierNode id:
                        {
                            if (context.Markers.TryGetValue(id.Name, out var func))
                                markers.Add(func());
                            else
                                propertiesAndVariants.Add(id);
                        }
                        break;

                    case QuotedNode: // Label
                        propertiesAndVariants.Add(item);
                        break;

                    case BinaryNode bn when bn.Type == BinaryOperatortype.Concatenate:
                        {
                            // This should evaluate to a name of a variant
                            string name = EvaluateName(bn, context);
                            if (name is null)
                                continue;
                            if (context.Markers.TryGetValue(name, out var func))
                                markers.Add(func());
                            else
                                propertiesAndVariants.Add(new IdentifierNode(new(bn.Location, name.AsMemory())));
                        }
                        break;

                    default:
                        context.Diagnostics?.Post(item.Location, ErrorCodes.ExpectedVariantName);
                        break;
                }
            }
            if (markers.Count > 0)
            {
                if (segments.Count > 0)
                    segments[^1].EndMarkers = [.. markers];
            }

            // Create the wire
            CreateWire(startPin, segments, propertiesAndVariants, endPin, context);
        }
        private static void CreateWire(PinReference startPin, List<WireSegmentInfo> segments, List<SyntaxNode> propertiesAndVariants, PinReference endPin, EvaluationContext context)
        {
            string wireName = context.GetWireName();
            if (segments.Count > 0)
            {
                // Create anonymous points if the pin isn't specified
                if (startPin is not null)
                    context.Circuit.Add(new PinOrientationConstraint($"{wireName}.s", startPin, -1, segments[0], false));
                if (endPin is not null)
                    context.Circuit.Add(new PinOrientationConstraint($"{wireName}.e", endPin, 0, segments[^1], true));

                // Create the wire
                var result = new Wire(wireName, startPin, segments, endPin);
                context.NotifyDrawable(result);
                context.CurrentScope.ApplyDefaults("wire", result, context.Diagnostics);

                // Apply properties and variants
                ApplyPropertiesAndVariants(result, propertiesAndVariants, context);
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
                            Length = MinimumWireLength
                        };

                        if (direction.Angle is not null)
                        {
                            double angle = EvaluateAsDouble(direction.Angle, context, 0.0);
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
                            if (direction.Distance is UnaryNode unary && unary.Type == UnaryOperatortype.Positive)
                                segment.Length = EvaluateAsDouble(unary.Argument, context, MinimumWireLength);
                            else
                            {
                                segment.Length = EvaluateAsDouble(direction.Distance, context, MinimumWireLength);
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
            // Split into the component and its properties
            SyntaxNode[] properties = null;
            if (component is PropertyListNode pln)
            {
                properties = pln.Properties;
                component = pln.Subject;
            }

            // Evaluate the full name of the component
            ICircuitPresence presence;
            if (component is BinaryNode bn && bn.Type == BinaryOperatortype.Backtrack)
            {
                // This should be treated as an anonymous component in the past
                string name = EvaluateName(bn.Left, context);
                if (name is null)
                    return null;
                int goBack = EvaluateAsInteger(bn.Right, context, 1);

                // Backtrack the anonymous component
                if (!context.TryGetBacktrackedAnonymousComponent(bn.Left.Location, name, goBack, out presence))
                    return null;
            }
            else
            {
                // Regular naming
                string name = EvaluateName(component, context);
                if (name is null)
                    return null;

                // If the name is an anonymous point, then let's 
                if (context.QueuedPoints.Count > 0 && name.Equals(PointFactory.Key))
                {
                    var pt = context.QueuedPoints.Dequeue();
                    return pt;
                }

                // Validate the name of the component
                if (!IsValidName(name))
                {
                    context.Diagnostics?.Post(component.Location, ErrorCodes.InvalidName, name);
                    return null;
                }

                // Expand the name
                name = context.GetFullname(name);

                // Get the item
                if (!context.Circuit.TryGetValue(name, out presence))
                {
                    presence = context.Factory.Create(name, context.Options, context.CurrentScope, context.Diagnostics);
                    if (presence is null)
                    {
                        context.Diagnostics?.Post(component.Location, ErrorCodes.CouldNotCreateComponentForName, name);
                        return null;
                    }
                    context.Circuit.Add(presence);

                    if (presence is Subcircuit.Instance inst)
                        inst.ApplyDefaultProperties(context);
                }

            }

            // Register the source
            presence.Sources.Add(component.Location);

            // Apply properties and variants
            if (presence is IDrawable drawable)
            {
                context.NotifyDrawable(drawable);
                if (properties is not null)
                    ApplyPropertiesAndVariants(drawable, properties, context);
            }
            return presence;
        }
        private static ILocatedPresence ProcessVirtualComponent(SyntaxNode component, VirtualChainConstraints flags, EvaluationContext context)
        {
            // Evaluate and validate the name
            if (component is BinaryNode bn && bn.Type == BinaryOperatortype.Backtrack)
            {
                // This should be treated as an anonymous component in the past
                string name = EvaluateName(bn.Left, context);
                if (name is null)
                    return null;
                int goBack = EvaluateAsInteger(bn.Right, context, 1);

                // Get the backtracked anonymous component
                if (!context.TryGetBacktrackedAnonymousComponent(bn.Left.Location, name, goBack, out var presence))
                    return null;
                if (presence is not ILocatedPresence lp)
                {
                    context.Diagnostics?.Post(bn.Left.Location, ErrorCodes.ComponentCannotChangeLocation);
                    return null;
                }
                return lp;
            }
            else
            {
                string name = EvaluateFilter(component, context);
                if (name is null)
                    return null;

                // Return the result
                var result = new AlignedComponents(context.GetVirtualName(), name, flags);
                context.Circuit.Add(result);
                return result;
            }
        }
        private static ILocatedPresence ProcessVirtualComponent(SyntaxNode component, SyntaxNode pin, VirtualChainConstraints flags, EvaluationContext context)
        {
            // Evaluate and validate the name
            string name = EvaluateFilter(component, context);
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
            string name = EvaluateFilter(component, context);
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

        /// <summary>
        /// Evaluates a syntax node as a component name.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="context">The context.</param>
        /// <returns>Returns the name.</returns>
        public static string EvaluateName(SyntaxNode node, EvaluationContext context)
        {
            switch (node)
            {
                case LiteralNode literalNode:
                    return literalNode.Value.ToString();

                case ConstantNode number:
                    return number.ToString();

                case BinaryNode binaryNode when binaryNode.Type == BinaryOperatortype.Concatenate:
                    string left = EvaluateName(binaryNode.Left, context);
                    string right = EvaluateName(binaryNode.Right, context);
                    if (left is null || right is null)
                        return null;
                    return left + right;

                case BinaryNode binaryNode when binaryNode.Type == BinaryOperatortype.Backtrack:
                    left = EvaluateName(binaryNode.Left, context);
                    right = EvaluateName(binaryNode.Right, context);
                    if (left is null || right is null)
                        return null;
                    return left + "~" + right;

                case BracketNode bracketNode when bracketNode.Left.Content.ToString() == "{" && bracketNode.Right.Content.ToString() == "}":
                    return ExpressionEvaluator.Evaluate(bracketNode.Value, context)?.ToString();

                default:
                    context.Diagnostics?.Post(node.Location, ErrorCodes.CouldNotResolveName, node.ToString());
                    return null;
            }
        }
        private static string EvaluateFilter(SyntaxNode node, EvaluationContext context)
        {
            // Evaluate and validate the name
            string name = EvaluateName(node, context);
            if (name is null)
                return null;
            if (!IsValidName(name, true))
            {
                context.Diagnostics?.Post(node.Location, ErrorCodes.InvalidName, name);
                return null;
            }

            // Resolve the name as a filter
            if (name != "wire")
            {
                if (name.Equals("*"))
                {
                    // This should match everything
                    name = $"^{context.GetFullname(".*", resolveAnonymous: false)}$";
                }
                else if (context.Factory.IsAnonymous(name))
                {
                    name = context.GetFullname(name, resolveAnonymous: false);
                    name = $"^{name}{DrawableFactoryDictionary.AnonymousSeparator}.*$";
                }
                else
                {
                    name = context.GetFullname(name, resolveAnonymous: false);
                    name = $"^{name.Replace("*", $"[^{DrawableFactoryDictionary.AnonymousSeparator}]*")}$";
                }
            }
            return name;
        }
        private static int EvaluateAsInteger(SyntaxNode node, EvaluationContext context, int defaultValue)
        {
            object result = EvaluateExpression(node, context);
            if (result is null)
                return defaultValue;
            if (result is int i)
                return i;
            if (result is double d)
            {
                if ((d - Math.Round(d)).IsZero())
                    return (int)Math.Round(d);
                else
                    context.Diagnostics?.Post(ErrorCodes.ExpectedInteger);
                return defaultValue;
            }
            return defaultValue;
        }
        private static double EvaluateAsDouble(SyntaxNode node, EvaluationContext context, double defaultValue)
        {
            object result = EvaluateExpression(node, context);
            if (result is null)
                return defaultValue;
            if (result is double d)
                return d;
            else if (result is int i)
                return i;
            else
            {
                context.Diagnostics?.Post(node.Location, ErrorCodes.ExpectedDouble);
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

        /// <summary>
        /// Evaluates a syntax node as an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The context.</param>
        /// <returns>The evaluated expression.</returns>
        public static object EvaluateExpression(SyntaxNode expression, EvaluationContext context)
        {
            switch (expression)
            {
                case BracketNode bracket when bracket.Left.Content.ToString() == "{":
                    return ExpressionEvaluator.Evaluate(bracket.Value, context);

                default:
                    return ExpressionEvaluator.Evaluate(expression, context);
            }
        }
    }
}
