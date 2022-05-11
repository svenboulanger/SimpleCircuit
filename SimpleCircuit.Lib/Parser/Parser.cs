using SimpleCircuit.Components;
using SimpleCircuit.Components.General;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

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
        public static void Parse(SimpleCircuitLexer lexer, ParsingContext context)
        {
            // Read until the end
            while (lexer.Type != TokenType.EndOfContent)
            {
                if (!ParseStatement(lexer, context))
                    lexer.Skip(~TokenType.Newline);
            }
        }

        /// <summary>
        /// Parses a SimpleCircuit statement.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <returns>Returns <c>true</c> if the statement was parsed successfully; otherwise, <c>false</c>.</returns>
        private static bool ParseStatement(SimpleCircuitLexer lexer, ParsingContext context)
        {
            switch (lexer.Type)
            {
                case TokenType.Word:
                    return ParseComponentChainStatement(lexer, context);

                case TokenType.Dot:
                    lexer.Next();
                    return ParseControlStatement(lexer, context);

                case TokenType.OpenParenthesis:
                    return ParseVirtualChainStatement(lexer, context);

                case TokenType.Dash:
                    return ParsePropertyAssignmentStatement(lexer, context);

                case TokenType.Newline:
                    lexer.Next();
                    return true;

                case TokenType.EndOfContent:
                    return true;

                default:
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        lexer, $"Unrecognized {lexer.Token}"));
                    lexer.Next();
                    return false;
            }
        }

        /// <summary>
        /// Parses a chain of components.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <returns>Returns <c>true</c> if the statement was parsed successfully; otherwise, <c>false</c>.</returns>
        private static bool ParseComponentChainStatement(SimpleCircuitLexer lexer, ParsingContext context)
        {
            // component[pin] '<' wires '>' [pin]component[pin] '<' wires '>' ... '>' [pin]component
            var component = ParseComponent(lexer, context);
            if (component == null)
                return false;
            IPin pinToWire = component.Pins[Math.Max(0, component.Pins.Count - 1)];
            string pinName;

            // Parse wires
            while (lexer.Check(TokenType.OpenIndex | TokenType.OpenBeak))
            {
                // Read a pin
                if (lexer.Check(TokenType.OpenIndex))
                {
                    pinName = ParsePin(lexer, context);
                    if (pinName == null)
                        return false;
                    pinToWire = component.Pins[pinName];
                }

                // Parse the wire itself
                List<WireInfo> wire;
                if (lexer.Check(TokenType.OpenBeak))
                {
                    wire = ParseWire(lexer, context);
                    if (wire == null)
                        lexer.Skip(~TokenType.CloseBeak | ~TokenType.Newline); // Skip the wire
                    else
                    {
                        // Parse an optional pin
                        pinName = null;
                        if (lexer.Check(TokenType.OpenIndex))
                        {
                            pinName = ParsePin(lexer, context);
                            if (pinName == null)
                                return false;
                        }

                        // Parse the next component
                        var nextComponent = ParseComponent(lexer, context);
                        if (nextComponent == null)
                            return false;

                        // Extract the previous pin
                        IPin wireToPin = pinName != null ? nextComponent.Pins[pinName] : nextComponent.Pins[0];

                        // String the pins together using wire segments
                        StringWiresTogether(pinToWire, wire, wireToPin, context);
                        component = nextComponent;

                        // To next component
                        pinToWire = component.Pins[Math.Max(0, component.Pins.Count - 1)];
                    }
                }
                else
                    break;
            }
            return true;
        }

        /// <summary>
        /// Parses a component.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <returns>The component.</returns>
        private static IDrawable ParseComponent(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Check(TokenType.Word))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Expected a component name"));
                return null;
            }

            // Get the full name of the component
            var name = lexer.ReadWhile(TokenType.Word | TokenType.Divide, shouldNotIncludeTrivia: true).ToString();
            string fullname = string.Join(DrawableFactoryDictionary.Separator, context.Section.Reverse().Union(new[] { name }));
            var component = context.GetOrCreate(fullname, context.Options);
            if (component == null)
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, $"Could not recognize and create a component for '{name}'"));
                return null;
            }

            // Labels
            HashSet<string> possibleVariants = null;
            if (lexer.Branch(TokenType.OpenParenthesis))
            {
                do
                {
                    // The comma is optional
                    lexer.Branch(TokenType.Comma);

                    // Parse
                    switch (lexer.Type)
                    {
                        case TokenType.String:
                            string txt = lexer.Token[1..^1].ToString();
                            if (component is ILabeled lbl)
                                lbl.Label = txt;
                            else
                            {
                                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PE001",
                                    $"Labeling is not possible for '{component.Name}' at line {lexer.Line}, column {lexer.Column}."));
                            }
                            lexer.Next();
                            break;

                        case TokenType.Dash:
                            lexer.Next();
                            if (lexer.Branch(TokenType.Word))
                                component.RemoveVariant(lexer.Token.ToString());
                            else
                            {
                                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                                    lexer, "Expected a variant"));
                                lexer.Skip(~TokenType.CloseParenthesis & ~TokenType.Comma & ~TokenType.Newline);
                            }
                            break;

                        case TokenType.Plus:
                            lexer.Next();
                            if (lexer.Check(TokenType.Word))
                                goto case TokenType.Word;
                            else
                            {
                                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                                    lexer, $"Expected a variant"));
                                lexer.Skip(~TokenType.CloseParenthesis & ~TokenType.Comma & ~TokenType.Newline);
                            }
                            break;

                        case TokenType.Word:
                            if (possibleVariants == null)
                            {
                                possibleVariants = new(StringComparer.OrdinalIgnoreCase);
                                component.CollectPossibleVariants(possibleVariants);
                            }
                            if (!possibleVariants.Contains(lexer.Token.ToString()))
                            {
                                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PE001",
                                    lexer, $"Could not recognize variant '{lexer.Token}' for '{component.Name}'"));
                            }
                            component.AddVariant(lexer.Token.ToString());
                            lexer.Next();
                            break;

                        default:
                            lexer.Next();
                            context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                                lexer, $"Expected label or variant for '{component.Name}'"));
                            break;
                    }
                }
                while (lexer.Type == TokenType.Comma);

                if (!lexer.Branch(TokenType.CloseParenthesis))
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        lexer, "Parenthesis mismatch, ')' expected"));
                }
            }
            return component;
        }

        /// <summary>
        /// Parses a wire.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The wire information.</returns>
        private static List<WireInfo> ParseWire(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.OpenBeak))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "A '<' is expected"));
            }

            // Read the direction of the wire
            List<WireInfo> wires = new();
            while (lexer.Type != TokenType.CloseBeak)
            {
                // Get the direction
                Vector2 orientation;
                switch (lexer.Token.ToString())
                {
                    case "n":
                    case "u": orientation = new(0, -1); lexer.Next(); break;
                    case "s":
                    case "d": orientation = new(0, 1); lexer.Next(); break;
                    case "e":
                    case "l": orientation = new(-1, 0); lexer.Next(); break;
                    case "w":
                    case "r": orientation = new(1, 0); lexer.Next(); break;
                    case "ne": orientation = Vector2.Normal(-Math.PI * 0.25); lexer.Next(); break;
                    case "nw": orientation = Vector2.Normal(-Math.PI * 0.75); lexer.Next(); break;
                    case "se": orientation = Vector2.Normal(Math.PI * 0.25); lexer.Next(); break;
                    case "sw": orientation = Vector2.Normal(Math.PI * 0.75); lexer.Next(); break;
                    case "0": orientation = new Vector2(); lexer.Next(); break;
                    case "a":
                        lexer.Next();
                        double angle = ParseDouble(lexer, context);
                        orientation = Vector2.Normal(-angle / 180.0 * Math.PI);
                        break;

                    default:
                        context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                            lexer, "Valid direction expected"));
                        lexer.Skip(~TokenType.Newline);
                        return null;
                }

                if (lexer.Branch(TokenType.Plus))
                {
                    if (!lexer.Branch(TokenType.Number, out var lengthToken))
                    {
                        context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                            lexer, "Length expected"));
                        return null;
                    }
                    double length = double.Parse(lengthToken.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
                    wires.Add(new WireInfo(orientation, double.NaN, length));
                }
                else if (lexer.Branch(TokenType.Number, out var lengthToken))
                {
                    double length = double.Parse(lengthToken.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
                    wires.Add(new WireInfo(orientation, length, context.Options.MinimumWireLength));
                }
                else
                {
                    wires.Add(new WireInfo(orientation, double.NaN, context.Options.MinimumWireLength));
                }
            }

            // Closing beak
            if (!lexer.Branch(TokenType.CloseBeak))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Bracket mismatch, '>' expected"));
            }
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

                var wire = new Wire($"W{++context.WireCount}", wires[i].MinimumLength, context.Options);
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
        private static string ParsePin(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.OpenIndex))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Pin expected"));
                return null;
            }

            // Pin name
            var name = lexer.ReadWhile(~TokenType.Newline & ~TokenType.CloseIndex & ~TokenType.Whitespace);

            if (!lexer.Branch(TokenType.CloseIndex))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Bracket mismatch, ']' expected"));
            }
            return name.ToString().Trim();
        }

        /// <summary>
        /// Parses an option.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <returns>Returns <c>true</c> if the statement was parsed successfully; otherwise, <c>false</c>.</returns>
        private static bool ParseControlStatement(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.Word, out var typeToken))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Expected control statement type"));
                return false;
            }

            switch (typeToken.ToString().ToLower())
            {
                case "subckt":
                    return ParseSubcircuitDefinition(lexer, context);

                case "symbol":
                    return ParseSymbolDefinition(lexer, context);

                case "section":
                    return ParseSectionDefinition(lexer, context);

                case "option":
                case "options":
                    return ParseOptions(lexer, context);

                default:
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        $"Could not recognize option '{lexer.Token}'"));
                    return false;
            }
        }

        private static bool ParseSubcircuitDefinition(SimpleCircuitLexer lexer, ParsingContext context)
        {
            // Read the name of the subcircuit
            if (!lexer.Branch(TokenType.Word, out var nameToken))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Subcircuit name expected"));
                return false;
            }

            // Create a new parsing context to separate our circuit
            var localContext = new ParsingContext() { Diagnostics = context.Diagnostics };
            List<IPin> ports = new();

            // Parse the pins
            while (lexer.Type != TokenType.Newline && lexer.Type != TokenType.EndOfContent)
            {
                // Parse the component
                var component = ParseComponent(lexer, localContext);
                if (component == null)
                    return false;

                // Find the pin
                IPin pin = null;
                if (lexer.Type == TokenType.OpenIndex)
                {
                    var pinName = ParsePin(lexer, localContext);
                    if (pinName == null)
                        return false;
                    pin = component.Pins[pinName];
                    if (pin == null)
                        return false;
                }
                pin ??= component.Pins[component.Pins.Count - 1];
                ports.Add(pin);
            }
            if (lexer.Type == TokenType.EndOfContent)
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    $"Unexpected end of code."));
                return false;
            }

            // Parse the netlist contents
            bool stillSubcircuit = true;
            while (stillSubcircuit && lexer.Type != TokenType.EndOfContent)
            {
                if (lexer.Branch(TokenType.Dot))
                {
                    if (lexer.Branch(TokenType.Word, "ends") || lexer.Branch(TokenType.Word, "endsubckt"))
                    {
                        lexer.Skip(~TokenType.Newline);
                        stillSubcircuit = false;
                    }
                    else if (!ParseControlStatement(lexer, localContext))
                        lexer.Skip(~TokenType.Newline);
                }
                else if (!ParseStatement(lexer, localContext))
                    lexer.Skip(~TokenType.Newline);
            }

            // Add a subcircuit definition to the context drawable factory
            var subckt = new Subcircuit(nameToken.ToString(), localContext.Circuit, ports, context.Diagnostics);
            context.Factory.Register(subckt);
            return true;
        }
        private static bool ParseSymbolDefinition(SimpleCircuitLexer lexer, ParsingContext context)
        {
            // read the symbol definition
            if (!lexer.Branch(TokenType.Word, "symbol"))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Expected symbol definition"));
                return false;
            }

            // Read the name of the subcircuit
            if (!lexer.Branch(TokenType.Word, out var nameToken))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Expected symbol name"));
                return false;
            }
            string symbolKey = nameToken.ToString();
            int line = lexer.Line;
            if (!lexer.Branch(TokenType.Newline))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, $"Unexpected '{lexer.Token}'"));
                return false;
            }

            // Read until a .ENDS and treat the contents as XML to be read
            StringBuilder xml = new();
            while (true)
            {
                if (lexer.Branch(TokenType.Dot))
                {
                    if (lexer.Branch(TokenType.Word, "ends") || lexer.Branch(TokenType.Word, "endsymbol"))
                    {
                        lexer.Skip(~TokenType.Newline);
                        break;
                    }
                    else
                    {
                        xml.Append('.');
                        xml.AppendLine(lexer.ReadWhile(~TokenType.Newline).ToString());
                    }
                }
                else
                    xml.AppendLine(lexer.ReadWhile(~TokenType.Newline).ToString());
                lexer.Next();
            }

            // Parse the XML
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml($"<symbol>{Environment.NewLine}{xml}{Environment.NewLine}</symbol>");
            }
            catch (XmlException ex)
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, $"XML",
                    $"{ex.Message} at line {line + ex.LineNumber - 1}, column {line + ex.LinePosition}."));
                return false;
            }

            // Build the XML symbol
            var symbol = new XmlDrawable(symbolKey, doc.DocumentElement, context.Diagnostics);
            context.Factory.Register(symbol);
            return true;
        }
        private static bool ParseSectionDefinition(SimpleCircuitLexer lexer, ParsingContext context)
        {
            // Read the name of the section
            if (!lexer.Branch(TokenType.Word, out var nameToken))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Section name expected"));
                return false;
            }
            lexer.Skip(~TokenType.Newline);

            // Parse section contents
            context.Section.Push(nameToken.ToString());
            bool stillSubcircuit = true;
            while (stillSubcircuit && lexer.Type != TokenType.EndOfContent)
            {
                if (lexer.Branch(TokenType.Dot))
                {
                    if (lexer.Branch(TokenType.Word, "ends") || lexer.Branch(TokenType.Word, "endsection"))
                    {
                        lexer.Skip(~TokenType.Newline);
                        stillSubcircuit = false;
                    }
                    else if (!ParseControlStatement(lexer, context))
                        lexer.Skip(~TokenType.Newline);
                }
                else if (!ParseStatement(lexer, context))
                    lexer.Skip(~TokenType.Newline);
            }
            lexer.Skip(~TokenType.Newline);
            context.Section.Pop();
            return true;
        }
        private static bool ParseOptions(SimpleCircuitLexer lexer, ParsingContext context)
        {
            while (lexer.Branch(TokenType.Word, out var nameToken))
            {
                // Find the property on the global options
                var member = typeof(Options).GetProperty(nameToken.ToString(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (member == null || !member.CanWrite)
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        $"Could not recognize global option '{nameToken}' at line {lexer.Line}, column {lexer.Column}."));
                    return false;
                }

                if (!lexer.Branch(TokenType.Equals))
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PE001",
                        lexer, "Assignment '=' expected"));
                }
                // Parse depending on the type
                if (member.PropertyType == typeof(bool))
                    member.SetValue(context.Options, ParseBoolean(lexer, context));
                else if (member.PropertyType == typeof(double))
                {
                    double result = ParseDouble(lexer, context);
                    if (double.IsNaN(result))
                        return false;
                    member.SetValue(context.Options, result);
                }
                else if (member.PropertyType == typeof(string))
                {
                    member.SetValue(context.Options, ParseString(lexer, context));
                }
                else
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        lexer, $"Cannot recognize property type {member.PropertyType.Name}"));
                }
            }

            if (lexer.Type != TokenType.EndOfContent && !lexer.Check(TokenType.Newline))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    $"Unexpected {lexer.Type} at line {lexer.Line}, column {lexer.Column}."));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses a virtual chain of coordinates.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The context.</param>
        private static bool ParseVirtualChainStatement(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.OpenParenthesis))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Virtual chain statement expected"));
                return false;
            }

            if (!lexer.Branch(TokenType.Word, out var directionToken))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Virtual chain direction expected"));
                return false;
            }

            bool x = false, y = false;
            switch (directionToken.ToString().ToLower())
            {
                case "x": x = true; break;
                case "y": y = true; break;
                case "xy":
                case "yx": x = true; y = true; break;
                default:
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        lexer, "Cannot not recognize virtual chain direction."));
                    return false;
            }

            // component[pin] '<' wires '>' [pin]component[pin] '<' wires '>' ... '>' [pin]component
            var component = ParseComponent(lexer, context);
            IPin pinToWire = component.Pins[component.Pins.Count - 1];
            string pinName;

            // Parse wires
            while (lexer.Check(TokenType.OpenIndex | TokenType.OpenBeak))
            {
                // Read a pin
                if (lexer.Check(TokenType.OpenIndex))
                {
                    pinName = ParsePin(lexer, context);
                    if (pinName == null)
                        return false;
                    pinToWire = component.Pins[pinName];
                }

                // Parse the wire itself
                var wire = ParseWire(lexer, context);
                if (wire == null)
                    return false;

                // Parse an optional pin
                pinName = null;
                if (lexer.Check(TokenType.OpenIndex))
                {
                    pinName = ParsePin(lexer, context);
                    if (pinName == null)
                        return false;
                }

                // Parse the next component
                var nextComponent = ParseComponent(lexer, context);
                if (nextComponent == null)
                    return false;

                // Extract the previous pin
                IPin wireToPin = pinName != null ? nextComponent.Pins[pinName] : nextComponent.Pins[0];

                // String the pins together using wire segments
                if (x)
                    StringVirtualWiresTogether(pin => pin.X, new(1, 0), pinToWire, wire, wireToPin, context);
                if (y)
                    StringVirtualWiresTogether(pin => pin.Y, new(0, 1), pinToWire, wire, wireToPin, context);

                // To next component
                component = nextComponent;
                pinToWire = component.Pins[component.Pins.Count - 1];
            }

            if (!lexer.Branch(TokenType.CloseParenthesis))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PE001",
                    lexer, "Parenthesis mismatch, ')' expected"));
            }
            return true;
        }
        private static void StringVirtualWiresTogether(Func<IPin, string> variableFunc, Vector2 normal, IPin pinToWire, List<WireInfo> wires, IPin wireToPin, ParsingContext context)
        {
            // We will go through each wire and only consider those that have an effect on the wires
            string lastNode = variableFunc(pinToWire);
            for (int i = 0; i < wires.Count; i++)
            {
                double dot = wires[i].Orientation.Dot(normal);

                // Create an intermediary point
                string node;
                if (i < wires.Count - 1)
                    node = $"virtual.{++context.VirtualCoordinateCount}";
                else
                    node = variableFunc(wireToPin);

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

        private static bool ParsePropertyAssignmentStatement(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.Dash))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Property assignment statement expected"));
                return false;
            }

            while (lexer.Check(~TokenType.Newline))
            {
                // Parse the component
                var component = ParseComponent(lexer, context);
                if (component == null)
                    return false;

                // Property
                if (!lexer.Branch(TokenType.Dot))
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        lexer, "Expected '.'"));
                    return false;
                }
                if (!lexer.Branch(TokenType.Word, out var propertyToken))
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        lexer, "Property expected"));
                    return false;
                }

                // Find the property on the component
                var member = component.GetType().GetProperty(propertyToken.ToString(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (member == null || !member.CanWrite)
                {
                    // Check if we can maybe resolve to a variant
                    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    component.CollectPossibleVariants(set);
                    if (!set.Contains(propertyToken.ToString()))
                    {
                        context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PE001",
                            $"Cannot find property or variant '{propertyToken}' for {component.Name}."));
                    }

                    // Treat it as a boolean
                    if (!lexer.Branch(TokenType.Equals))
                    {
                        context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PW001",
                            lexer, "Assignment operator '=' expected"));
                    }
                    var value = ParseBoolean(lexer, context);
                    if (value == null)
                        return false;
                    if (value == true)
                        component.AddVariant(propertyToken.ToString());
                    else
                        component.RemoveVariant(propertyToken.ToString());
                    return true;
                }

                // Equal sign
                if (!lexer.Branch(TokenType.Equals))
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "PW001",
                        lexer, "Assignment operator '=' expected"));
                }    

                if (member.PropertyType == typeof(bool))
                    member.SetValue(component, ParseBoolean(lexer, context));
                else if (member.PropertyType == typeof(double))
                {
                    double result = ParseDouble(lexer, context);
                    if (double.IsNaN(result))
                        return false;
                    member.SetValue(component, result);
                }
                else if (member.PropertyType == typeof(int))
                {
                    double result = ParseDouble(lexer, context);
                    if (double.IsNaN(result))
                        return false;
                    member.SetValue(component, (int)Math.Round(result, MidpointRounding.AwayFromZero));
                }
                else if (member.PropertyType == typeof(string))
                {
                    member.SetValue(component, ParseString(lexer, context));
                }
                else
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        lexer, $"Cannot recognize property type {member.PropertyType.Name}"));
                    return false;
                }
            }
            return true;
        }
        private static bool? ParseBoolean(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.Word, "true", StringComparison.Ordinal))
            {
                if (!lexer.Branch(TokenType.Word, "false", StringComparison.Ordinal))
                {
                    context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                        lexer, "Boolean expected"));
                    return null;
                }
                else
                    return false;
            }
            else
                return true;
        }
        private static double ParseDouble(SimpleCircuitLexer lexer, ParsingContext context)
        {
            bool inverted = false;
            if (lexer.Branch(TokenType.Dash))
                inverted = true;
            if (!lexer.Branch(TokenType.Number, out var numberToken))
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "Number expected"));
                return double.NaN;
            }
            double result = double.Parse(numberToken.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
            if (inverted)
                result = -result;
            return result;
        }
        private static string ParseString(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (lexer.Branch(TokenType.String, out var stringToken))
            {
                return stringToken[1..^1].ToString();
            }
            else
            {
                context.Diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "PE001",
                    lexer, "String expected"));
                return null;
            }
        }
    }
}
