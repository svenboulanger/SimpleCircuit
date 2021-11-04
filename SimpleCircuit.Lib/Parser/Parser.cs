using SimpleCircuit.Components;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Parser for SimpleCircuit code.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// Parses SimpleCircuit code.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The context.</param>
        public static void Parse(Lexer lexer, ParsingContext context)
        {
            while (!lexer.EndOfContent)
                ParseCode(lexer, context);
            
        }

        /// <summary>
        /// Parses SimpleCircuit code.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        private static bool ParseCode(Lexer lexer, ParsingContext context)
        {
            while (lexer.Type != TokenType.EndOfContent)
            {
                switch (lexer.Type)
                {
                    case TokenType.Word:
                        ParseComponentChain(lexer, context);
                        break;

                    case TokenType.Dot:
                        if (ParseOption(lexer, context))
                            return true;
                        break;

                    case TokenType.OpenParenthesis:
                        ParseVirtualChain(lexer, context);
                        break;

                    case TokenType.Dash:
                        ParseAssignments(lexer, context);
                        break;

                    case TokenType.Whitespace:
                        lexer.SkipWhile(TokenType.Whitespace);
                        break;

                    case TokenType.Newline:
                    case TokenType.Comment:
                        lexer.Next();
                        break;

                    default:
                        context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                            $"Unrecognized {lexer.Content} at line {lexer.Line}, column {lexer.Column}."));
                        lexer.SkipWhile(~TokenType.Newline);
                        break;
                }
            }
            return false;
        }

        /// <summary>
        /// Parses a chain of components.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        private static void ParseComponentChain(Lexer lexer, ParsingContext context)
        {
            // component[pin] '<' wires '>' [pin]component[pin] '<' wires '>' ... '>' [pin]component
            var component = ParseComponent(lexer, context);
            if (component == null)
            {
                lexer.SkipWhile(~TokenType.Newline);
                return;
            }
            IPin pinToWire = component.Pins[Math.Max(0, component.Pins.Count - 1)];
            string pinName;
            
            // Parse wires
            lexer.SkipWhile(TokenType.Whitespace);
            while (lexer.Check(TokenType.OpenIndex | TokenType.OpenBeak))
            {
                // Read a pin
                if (lexer.Check(TokenType.OpenIndex))
                {
                    pinName = ParsePin(lexer, context);
                    if (pinName == null)
                    {
                        lexer.SkipWhile(~TokenType.Newline);
                        return;
                    }
                    pinToWire = component.Pins[pinName];
                }

                // Parse the wire itself
                List<WireInfo> wire;
                if (lexer.Check(TokenType.OpenBeak))
                {
                    wire = ParseWire(lexer, context);
                    if (wire == null)
                    {
                        lexer.SkipWhile(~TokenType.Newline);
                        return;
                    }
                }
                else
                    break;

                // Parse an optional pin
                lexer.SkipWhile(TokenType.Whitespace);
                pinName = null;
                if (lexer.Check(TokenType.OpenIndex))
                {
                    pinName = ParsePin(lexer, context);
                    if (pinName == null)
                        return;
                }

                // Parse the next component
                var nextComponent = ParseComponent(lexer, context);
                if (nextComponent == null)
                {
                    lexer.SkipWhile(~TokenType.Newline);
                    return;
                }
                lexer.SkipWhile(TokenType.Whitespace);

                // Extract the previous pin
                IPin wireToPin = pinName != null ? nextComponent.Pins[pinName] : nextComponent.Pins[0];

                // String the pins together using wire segments
                StringWiresTogether(pinToWire, wire, wireToPin, context);

                // To next component
                component = nextComponent;
                pinToWire = component.Pins[Math.Max(0, component.Pins.Count - 1)];
            }
        }

        /// <summary>
        /// Parses a component.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <returns>The component.</returns>
        private static IDrawable ParseComponent(Lexer lexer, ParsingContext context)
        {
            if (!lexer.Expect(TokenType.Word, null, "PE001", context.Diagnostics))
                 return null;
            var component = context.GetOrCreate(lexer.Content);
            if (component == null)
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    $"Could not recognize and create a component for '{lexer.Content}' at line {lexer.Line}, column {lexer.Column}."));
                lexer.Next();
                return null;
            }
            lexer.Next();

            // Labels
            lexer.SkipWhile(TokenType.Whitespace);
            if (lexer.Branch(TokenType.OpenParenthesis))
            {
                lexer.SkipWhile(TokenType.Whitespace);
                if (!lexer.Expect(TokenType.String, null, "PE001", context.Diagnostics))
                    lexer.SkipWhile(~TokenType.CloseParenthesis & ~TokenType.Newline);
                string label = lexer.Content.Substring(1, lexer.Content.Length - 2);
                lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);

                if (lexer.Expect(TokenType.CloseParenthesis, ")", "PE001", context.Diagnostics))
                    lexer.Next();

                if (component is ILabeled lbl)
                    lbl.Label = label;
                else
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PE001",
                        $"Labeling is not possible for '{component.Name}'"));
                }
            }
            return component;
        }

        /// <summary>
        /// Parses a wire.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The wire information.</returns>
        private static List<WireInfo> ParseWire(Lexer lexer, ParsingContext context)
        {
            lexer.SkipWhile(TokenType.Whitespace);
            if (!lexer.Expect(TokenType.OpenBeak, "<", "PE001", context.Diagnostics))
                return null;
            lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);

            // Read the direction of the wire
            List<WireInfo> wires = new();
            while (lexer.Type != TokenType.CloseBeak)
            {
                // Get the direction
                Vector2 orientation;
                switch (lexer.Content)
                {
                    case "n":
                    case "u": orientation = new(0, -1); break;
                    case "s":
                    case "d": orientation = new(0, 1); break;
                    case "e":
                    case "l": orientation = new(-1, 0); break;
                    case "w":
                    case "r": orientation = new(1, 0); break;
                    case "ne": orientation = Vector2.Normal(-Math.PI * 0.25); break;
                    case "nw": orientation = Vector2.Normal(-Math.PI * 0.75); break;
                    case "se": orientation = Vector2.Normal(Math.PI * 0.25); break;
                    case "sw": orientation = Vector2.Normal(Math.PI * 0.75); break;
                    case "0": orientation = new Vector2(); break;
                    case "a":
                        lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
                        bool inverted = false;
                        if (lexer.Branch(TokenType.Dash))
                            inverted = true;
                        if (!lexer.Expect(TokenType.Number, null, "PE001", context.Diagnostics))
                            return null;
                        double angle = double.Parse(lexer.Content);
                        orientation = Vector2.Normal((inverted ? angle : -angle) / 180.0 * Math.PI);
                        break;

                    default:
                        context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                            $"Valid direction expected at line {lexer.Line}, column {lexer.Column}."));
                        lexer.SkipWhile(~TokenType.Newline);
                        return null;
                }
                lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);

                if (lexer.Branch(TokenType.Plus))
                {
                    lexer.SkipWhile(TokenType.Whitespace);
                    if (!lexer.Expect(TokenType.Number, null, "PE001", context.Diagnostics))
                        return null;
                    double length = double.Parse(lexer.Content);
                    wires.Add(new WireInfo(orientation, double.NaN, length));
                    lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
                }
                else if (lexer.Check(TokenType.Number))
                {
                    double length = double.Parse(lexer.Content);
                    wires.Add(new WireInfo(orientation, length, GlobalOptions.MinimumWireLength));
                    lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
                }
                else
                {
                    wires.Add(new WireInfo(orientation, double.NaN, GlobalOptions.MinimumWireLength));
                }
            }

            // Closing beak
            lexer.Expect(TokenType.CloseBeak, ">", "PE001", context.Diagnostics);
            lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
            return wires;
        }

        /// <summary>
        /// Strings two pins together using wires.
        /// </summary>
        /// <param name="pinToWire">The first pin.</param>
        /// <param name="wires">The pin coming before the wire.</param>
        /// <param name="wireToPin">The pin coming after the wire.</param>
        /// <param name="context">The parsing context.</param>
        private static void StringWiresTogether(IPin pinToWire, List<WireInfo> wires, IPin wireToPin, ParsingContext context)
        {
            IPin last = pinToWire;
            var orientation = new Vector2(1, 0);
            for (int i = 0; i < wires.Count; i++)
            {
                if (wires[i].Orientation.Equals(new Vector2()))
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        $"Cannot use short-hand notation '0' in non-virtual chains."));
                    return;
                }

                var wire = new Wire($"W{++context.WireCount}", wires[i].MinimumLength);
                context.Circuit.Add(wire);

                // Alias the starting point of this wire to the last wire end
                last.Connections++;
                wire.Pins[0].Connections++;
                context.Circuit.Add(
                    new OffsetConstraint($"{wire.Name}.sc.x", last.X, wire.Pins[0].X),
                    new OffsetConstraint($"{wire.Name}.sc.y", last.Y, wire.Pins[0].Y));

                // Apply a direction for this wire
                orientation = wires[i].Orientation;
                ((IOrientedPin)wire.Pins[0]).ResolveOrientation(-orientation, context.Diagnostics);
                if (last is IOrientedPin op1)
                    op1.ResolveOrientation(orientation, context.Diagnostics);

                // If the wire has fixed length, fix it!
                if (!double.IsNaN(wires[i].Length))
                    wire.Fix(wires[i].Length);

                // Update the pin
                last = wire.Pins[1];
            }

            // Do the last pin
            last.Connections++;
            wireToPin.Connections++;
            context.Circuit.Add(
                new OffsetConstraint($"{last.Owner.Name}.ec.x", last.X, wireToPin.X),
                new OffsetConstraint($"{last.Owner.Name}.ec.y", last.Y, wireToPin.Y));
            if (wireToPin is IOrientedPin op)
                op.ResolveOrientation(-orientation, context.Diagnostics);
        }

        /// <summary>
        /// Parses a single pin name.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The name of the pin.</returns>
        private static string ParsePin(Lexer lexer, ParsingContext context)
        {
            // Opening indexer
            if (!lexer.Expect(TokenType.OpenIndex, "[", "PE001", context.Diagnostics))
                return null;
            lexer.Next();

            // Pin name
            lexer.SkipWhile(TokenType.Whitespace);
            if (!lexer.Expect(TokenType.Word, null, "PE001", context.Diagnostics))
                return null;
            string name = lexer.Content;
            lexer.Next();

            // Closing indexer
            lexer.SkipWhile(TokenType.Whitespace);
            if (!lexer.Expect(TokenType.CloseIndex, "]", "PE001", context.Diagnostics))
                return null;
            lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
            return name;
        }

        /// <summary>
        /// Parses an option.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <returns>Returns <c>true</c> if the end of a subcircuit definition was found.</returns>
        private static bool ParseOption(Lexer lexer, ParsingContext context)
        {
            if (!lexer.Expect(TokenType.Dot, ".", "PE001", context.Diagnostics))
                return false;
            lexer.Next();
            if (!lexer.Expect(TokenType.Word, null, "PE001", context.Diagnostics))
            {
                lexer.SkipWhile(~TokenType.Newline);
                return false;
            }
            switch (lexer.Content.ToLower())
            {
                case "subckt":
                    lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
                    ParseSubcircuitDefinition(lexer, context);
                    return false;

                case "ends":
                    lexer.Next(); lexer.SkipWhile(~TokenType.Newline);
                    return true;

                case "option":
                case "options":
                    lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
                    ParseGlobalOptions(lexer, context);
                    return false;

                default:
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        $"Could not recognize option '{lexer.Content}'"));
                    lexer.SkipWhile(~TokenType.Newline);
                    return false;
            }
        }

        /// <summary>
        /// Parses a subcircuit definition.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        private static void ParseSubcircuitDefinition(Lexer lexer, ParsingContext context)
        {
            // Read the name of the subcircuit
            if (!lexer.Expect(TokenType.Word, null, "PE001", context.Diagnostics))
            {
                lexer.SkipWhile(~TokenType.Newline);
                return;
            }
            string subcktName = lexer.Content;
            lexer.Next();

            // Check if the subcircuit doesn't already exist
            if (context.Definitions.Search(subcktName, out _))
            {
                lexer.SkipWhile(TokenType.All);
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    $"The subcircuit definition '{subcktName}' already exists."));
                return;
            }

            // Create a new parsing context to separate our circuit
            var localContext = new ParsingContext() { Diagnostics = context.Diagnostics };
            var definition = new SubcircuitDefinition(localContext.Circuit);

            // Parse the pins
            lexer.SkipWhile(TokenType.Whitespace);
            while (lexer.Type != TokenType.Newline && lexer.Type != TokenType.EndOfContent)
            {
                // Parse the component
                if (!lexer.Expect(TokenType.Word, null, "PE001", context.Diagnostics))
                {
                    lexer.SkipWhile(~TokenType.Newline);
                    return;
                }
                var component = localContext.GetOrCreate(lexer.Content);
                if (component == null)
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        $"Could not get a component for the name '{lexer.Content}'."));
                    lexer.SkipWhile(TokenType.All);
                    return;
                }
                lexer.Next();

                // Find the pin
                IPin pin = null;
                if (lexer.Type == TokenType.OpenIndex)
                {
                    pin = component.Pins[ParsePin(lexer, context)];
                    if (pin == null)
                    {
                        lexer.SkipWhile(TokenType.All);
                        return;
                    }
                }
                pin ??= component.Pins[component.Pins.Count - 1];
                definition.Ports.Add(pin);

                lexer.SkipWhile(TokenType.Whitespace);
            }
            if (lexer.EndOfContent)
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    $"Unexpected end of code."));
                return;
            }
            lexer.Next();

            // Parse the netlist contents
            if (!ParseCode(lexer, localContext))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    $"No .ends detected for subcircuit '{subcktName}'."));
                lexer.SkipWhile(~TokenType.Whitespace);
                return;
            }
            context.Definitions.Add(subcktName, definition);
            definition.Definition.Solve(context.Diagnostics);
        }

        private static void ParseGlobalOptions(Lexer lexer, ParsingContext context)
        {
            while (lexer.Check(TokenType.Word))
            {
                string property = lexer.Content;
                lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);

                // Find the property on the global options
                var member = typeof(GlobalOptions).GetProperty(property, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (member == null || !member.CanWrite)
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        $"Could not recognize global option '{property}' at line {lexer.Line}, column {lexer.Column}."));
                    lexer.SkipWhile(~TokenType.Newline);
                    return;
                }

                if (lexer.Branch(TokenType.Equals))
                    lexer.SkipWhile(TokenType.Whitespace);

                // Parse depending on the type
                if (member.PropertyType == typeof(bool))
                    member.SetValue(null, ParseBoolean(lexer, context));
                else if (member.PropertyType == typeof(double))
                {
                    double result = ParseDouble(lexer, context);
                    if (double.IsNaN(result))
                        return;
                    member.SetValue(null, result);
                }
                else if (member.PropertyType == typeof(string))
                {
                    member.SetValue(null, ParseString(lexer, context));
                }
                else
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        $"Cannot recognize property type {member.PropertyType.Name} at line {lexer.Line}, column {lexer.Column}."));
                }
            }
            if (!lexer.EndOfContent && !lexer.Check(TokenType.Newline))
            {
                lexer.SkipWhile(TokenType.Whitespace);
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    $"Unexpected {lexer.Type} at line {lexer.Line}, column {lexer.Column}."));
            }
        }

        /// <summary>
        /// Parses a virtual chain of coordinates.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The context.</param>
        private static void ParseVirtualChain(Lexer lexer, ParsingContext context)
        {
            if (!lexer.Expect(TokenType.OpenParenthesis, "(", "PE001", context.Diagnostics))
            {
                lexer.SkipWhile(~TokenType.Newline);
                return;
            }
            lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
            if (!lexer.Expect(TokenType.Word, null, "PE001", context.Diagnostics))
            {
                lexer.SkipWhile(~TokenType.Newline);
                return;
            }
            Vector2 direction;
            switch (lexer.Content.ToLower())
            {
                case "x": direction = new(1, 0); break;
                case "y": direction = new(0, 1); break;
                default:
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        $"Could not recognize virtual chain direction."));
                    direction = new();
                    break;
            }
            lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);

            if (direction.Equals(new Vector2()))
            {
                lexer.SkipWhile(~TokenType.CloseParenthesis);
            }
            else
            {
                // component[pin] '<' wires '>' [pin]component[pin] '<' wires '>' ... '>' [pin]component
                var component = ParseComponent(lexer, context);
                IPin pinToWire = component.Pins[component.Pins.Count - 1];
                string pinName;

                // Parse wires
                lexer.SkipWhile(TokenType.Whitespace);
                while (lexer.Check(TokenType.OpenIndex | TokenType.OpenBeak))
                {
                    // Read a pin
                    if (lexer.Check(TokenType.OpenIndex))
                    {
                        pinName = ParsePin(lexer, context);
                        if (pinName == null)
                        {
                            lexer.SkipWhile(~TokenType.Newline);
                            return;
                        }
                        pinToWire = component.Pins[pinName];
                    }

                    // Parse the wire itself
                    var wire = ParseWire(lexer, context);
                    if (wire == null)
                    {
                        lexer.SkipWhile(~TokenType.Newline);
                        return;
                    }

                    // Parse an optional pin
                    lexer.SkipWhile(TokenType.Whitespace);
                    pinName = null;
                    if (lexer.Check(TokenType.OpenIndex))
                    {
                        pinName = ParsePin(lexer, context);
                        if (pinName == null)
                        {
                            lexer.SkipWhile(~TokenType.Newline);
                            return;
                        }
                    }

                    // Parse the next component
                    var nextComponent = ParseComponent(lexer, context);
                    if (nextComponent == null)
                    {
                        lexer.SkipWhile(~TokenType.Newline);
                        return;
                    }

                    // Extract the previous pin
                    IPin wireToPin = pinName != null ? nextComponent.Pins[pinName] : nextComponent.Pins[0];

                    // String the pins together using wire segments
                    StringVirtualWiresTogether(direction, pinToWire, wire, wireToPin, context);

                    // To next component
                    component = nextComponent;
                    pinToWire = component.Pins[component.Pins.Count - 1];
                }
            }

            if (lexer.Expect(TokenType.CloseParenthesis, ")", "PE001", context.Diagnostics))
            {
                lexer.Next();
                lexer.SkipWhile(TokenType.Whitespace);
            }
            else
                lexer.SkipWhile(~TokenType.Newline);
        }
        private static void StringVirtualWiresTogether(Vector2 direction, IPin pinToWire, List<WireInfo> wires, IPin wireToPin, ParsingContext context)
        {
            // We will go through each wire and only consider those that have an effect on the wires
            string lastNode = direction.X > 0 ? pinToWire.X : pinToWire.Y;
            for (int i = 0; i < wires.Count; i++)
            {
                double dot = wires[i].Orientation.Dot(direction);

                // Create an intermediary point
                string node;
                if (i < wires.Count - 1)
                    node = $"virtual.{++context.VirtualCoordinateCount}";
                else
                    node = direction.X > 0 ? wireToPin.X : wireToPin.Y;

                // Constrain these
                if (double.IsNaN(wires[i].Length) && !dot.IsZero())
                {
                    if (dot > 0)
                        context.Circuit.Add(new MinimumConstraint($"virtual.constraint.{++context.VirtualCoordinateCount}", lastNode, node, Math.Abs(dot) * wires[i].MinimumLength));
                    else
                        context.Circuit.Add(new MinimumConstraint($"virtual.constraint.{++context.VirtualCoordinateCount}", node, lastNode, Math.Abs(dot) * wires[i].MinimumLength));
                }
                else
                {
                    double length = dot.IsZero() ? 0.0 : dot * wires[i].Length;
                    context.Circuit.Add(new OffsetConstraint($"virtual.constraint.{++context.VirtualCoordinateCount}", lastNode, node, length));
                }

                lastNode = node;
            }
        }

        private static void ParseAssignments(Lexer lexer, ParsingContext context)
        {
            if (!lexer.Expect(TokenType.Dash, "-", "PE001", context.Diagnostics))
            {
                lexer.SkipWhile(~TokenType.Newline);
                return;
            }
            lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);

            while (lexer.Check(~TokenType.Newline))
            {
                // Parse the component
                var component = ParseComponent(lexer, context);
                if (component == null)
                {
                    lexer.SkipWhile(~TokenType.Newline);
                    return;
                }

                // Property
                if (!lexer.Expect(TokenType.Dot, ".", "PE001", context.Diagnostics))
                {
                    lexer.SkipWhile(~TokenType.Newline);
                    return;
                }
                lexer.Next();
                if (!lexer.Expect(TokenType.Word, null, "PE001", context.Diagnostics))
                {
                    lexer.SkipWhile(~TokenType.Newline);
                    return;
                }
                string property = lexer.Content;
                lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);

                // Find the property on the component
                var member = component.GetType().GetProperty(property);
                if (member == null || !member.CanWrite)
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PE001",
                        $"Cannot find property '{property}' for {component.Name}."));
                    lexer.SkipWhile(~TokenType.Newline);
                    return;
                }

                // Equal sign
                if (lexer.Branch(TokenType.Equals))
                    lexer.SkipWhile(TokenType.Whitespace);

                if (member.PropertyType == typeof(bool))
                    member.SetValue(component, ParseBoolean(lexer, context));
                else if (member.PropertyType == typeof(double))
                {
                    double result = ParseDouble(lexer, context);
                    if (double.IsNaN(result))
                        return;
                    member.SetValue(component, result);
                }
                else if (member.PropertyType == typeof(int))
                {
                    double result = ParseDouble(lexer, context);
                    if (double.IsNaN(result))
                        return;
                    member.SetValue(component, (int)Math.Round(result, MidpointRounding.AwayFromZero));
                }
                else if (member.PropertyType == typeof(string))
                {
                    member.SetValue(component, ParseString(lexer, context));
                }
                else
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        $"Cannot recognize property type {member.PropertyType.Name} at line {lexer.Line}, column {lexer.Column}."));
                }
            }
        }
        private static bool ParseBoolean(Lexer lexer, ParsingContext context)
        {
            if (!lexer.Expect(TokenType.Word, null, "PE001", context.Diagnostics))
            {
                lexer.SkipWhile(~TokenType.Newline);
                return false;
            }
            switch (lexer.Content.ToLower())
            {
                case "true": lexer.Next(); return true;
                case "false": lexer.Next(); return false;
                default:
                    lexer.SkipWhile(~TokenType.Newline);
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PE001",
                        $"Invalid boolean literal {lexer.Content} at line {lexer.Line}, column {lexer.Column}."));
                    return false;
            }
        }
        private static double ParseDouble(Lexer lexer, ParsingContext context)
        {
            bool inverted = false;
            if (lexer.Branch(TokenType.Dash))
                inverted = true;
            if (!lexer.Expect(TokenType.Number, null, "PE001", context.Diagnostics))
            {
                lexer.SkipWhile(~TokenType.Newline);
                return double.NaN;
            }
            double result = double.Parse(lexer.Content);
            if (inverted)
                result = -result;
            lexer.Next();
            return result;
        }
        private static string ParseString(Lexer lexer, ParsingContext context)
        {
            string result;
            switch (lexer.Type)
            {
                case TokenType.String:
                    result = lexer.Content.Substring(1, lexer.Content.Length - 2);
                    lexer.Next();
                    break;
                default:
                    result = "";
                    while (lexer.Check(~TokenType.Newline))
                    {
                        result += lexer.Content;
                        lexer.Next();
                    }
                    break;
            }
            return result;
        }
    }
}
