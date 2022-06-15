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
                    return ParseComponentChainStatement(lexer, context, (IPin pinToWire, WireInfo wireInfo, IPin wireToPin) => StringWiresTogether(pinToWire, wireInfo, wireToPin, context));

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
                    context.Diagnostics?.Post(new TokenDiagnosticMessage(lexer.Token, SeverityLevel.Error, "PE001", $"Unrecognized {lexer.Content}"));
                    lexer.Next();
                    return false;
            }
        }

        /// <summary>
        /// Parses a chain of components.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <param name="stringTogether">The action that is executed when wires should be </param>
        /// <returns>Returns <c>true</c> if the statement was parsed successfully; otherwise, <c>false</c>.</returns>
        private static bool ParseComponentChainStatement(SimpleCircuitLexer lexer, ParsingContext context, Action<IPin, WireInfo, IPin> stringTogether)
        {
            // component[pin] '<' wires '>' [pin]component[pin] '<' wires '>' ... '>' [pin]component
            var component = ParseComponent(lexer, context);
            if (component == null)
                return false;
            Token pinName;

            // Parse wires
            while (lexer.Check(TokenType.OpenIndex | TokenType.OpenBeak))
            {
                IPin pinToWire = null;

                // Read a pin
                if (lexer.Check(TokenType.OpenIndex))
                {
                    pinName = ParsePin(lexer, context);
                    if (pinName.Content.Length == 0)
                        return false;
                    if (!component.Pins.TryGetValue(pinName.Content.ToString().Trim(), out pinToWire))
                    {
                        if (context.Diagnostics?.Post(pinName, ErrorCodes.CannotFindPin, pinName.Content.ToString(), component.Name) == SeverityLevel.Error)
                            return false;
                    }
                }
                else if (component.Pins.Count == 0)
                {
                    if (context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.DoesNotHavePins, component.Name) == SeverityLevel.Error)
                        return false;
                }
                else
                {
                    // Select the last pin
                    pinToWire = component.Pins[^1];
                }

                // Parse the wire itself
                if (lexer.Check(TokenType.OpenBeak))
                {
                    var wireInfo = ParseWire(lexer, context);
                    if (wireInfo == null)
                        lexer.Skip(~TokenType.CloseBeak | ~TokenType.Newline); // Skip the wire
                    else
                    {
                        // Parse an optional pin
                        pinName = default;
                        if (lexer.Check(TokenType.OpenIndex))
                        {
                            pinName = ParsePin(lexer, context);
                            if (pinName.Content.Length == 0)
                                return false;
                        }

                        // Parse the next component
                        var nextComponent = ParseComponent(lexer, context);
                        if (nextComponent == null)
                            return false;

                        // Extract the pin for the next component
                        IPin wireToPin = null;
                        if (pinName.Content.Length > 0)
                        {
                            if (!nextComponent.Pins.TryGetValue(pinName.Content.ToString().Trim(), out wireToPin))
                            {
                                if (context.Diagnostics?.Post(pinName, ErrorCodes.CannotFindPin, pinName.Content.ToString(), nextComponent.Name) == SeverityLevel.Error)
                                    return false;
                            }
                        }
                        else if (nextComponent.Pins.Count == 0)
                        {
                            if (context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.DoesNotHavePins, nextComponent.Name) == SeverityLevel.Error)
                                return false;
                        }
                        else
                            wireToPin = nextComponent.Pins[0];

                        // String the pins together using wire segments
                        if (pinToWire != null && wireToPin != null)
                            stringTogether?.Invoke(pinToWire, wireInfo, wireToPin);
                        component = nextComponent;
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
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedComponentName);
                return null;
            }

            // Get the full name of the component
            var nameToken = lexer.ReadWhile(TokenType.Word | TokenType.Divide, shouldNotIncludeTrivia: true);
            string fullname = string.Join(DrawableFactoryDictionary.Separator, context.Section.Reverse().Union(new[] { nameToken.Content.ToString() }));
            var component = context.GetOrCreate(fullname, context.Options);
            if (component == null)
            {
                context.Diagnostics?.Post(nameToken, ErrorCodes.CouldNotRecognizeOrCreateComponent, nameToken.Content.ToString());
                return null;
            }

            // Labels
            HashSet<string> possibleVariants = null;
            if (lexer.Branch(TokenType.OpenParenthesis))
            {
                do
                {
                    // Parse
                    Token token;
                    switch (lexer.Type)
                    {
                        case TokenType.String:
                            string txt = lexer.Content[1..^1].ToString();
                            if (component is ILabeled lbl)
                                lbl.Label = txt;
                            else
                            {
                                context.Diagnostics?.Post(lexer.Token, ErrorCodes.LabelingNotPossible, component.Name);
                            }
                            lexer.Next();
                            break;

                        case TokenType.Dash:
                            lexer.Next();
                            if (lexer.Branch(TokenType.Word, out token))
                                component.RemoveVariant(token.Content.ToString());
                            else
                            {
                                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedVariant);
                                lexer.Skip(~TokenType.CloseParenthesis & ~TokenType.Comma & ~TokenType.Newline);
                            }
                            break;

                        case TokenType.Plus:
                            lexer.Next();
                            if (lexer.Check(TokenType.Word))
                                goto case TokenType.Word;
                            else
                            {
                                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedVariant);
                                lexer.Skip(~TokenType.CloseParenthesis & ~TokenType.Comma & ~TokenType.Newline);
                            }
                            break;

                        case TokenType.Word:
                            if (possibleVariants == null)
                            {
                                possibleVariants = new(StringComparer.OrdinalIgnoreCase);
                                component.CollectPossibleVariants(possibleVariants);
                            }
                            if (!possibleVariants.Contains(lexer.Content.ToString()))
                            {
                                if (context.Diagnostics?.Post(lexer.Token, ErrorCodes.CouldNotRecognizeVariant, lexer.Content.ToString(), component.Name) == SeverityLevel.Error)
                                    return null;
                            }
                            component.AddVariant(lexer.Content.ToString());
                            lexer.Next();
                            break;

                        case TokenType.Newline:
                            break;

                        default:
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedLabelOrVariant, component.Name);
                            lexer.Skip(~TokenType.CloseParenthesis & ~TokenType.Comma & ~TokenType.Newline);
                            break;
                    }
                }
                while (lexer.Branch(TokenType.Comma));

                if (!lexer.Branch(TokenType.CloseParenthesis))
                    context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.BracketMismatch, ")");
            }
            return component;
        }

        /// <summary>
        /// Parses a wire.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The wire information.</returns>
        private static WireInfo ParseWire(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.OpenBeak))
            {
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedWire);
                return null;
            }

            // Read the direction of the wire
            var wireInfo = new WireInfo()
            {
                JumpOverWires = context.Options.JumpOverWires
            };
            while (lexer.Type != TokenType.CloseBeak)
            {
                // Get the direction
                bool isSegment = true;
                Vector2 orientation = new();
                switch (lexer.Content.ToString())
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
                    case "0": lexer.Next(); break;
                    case "a":
                        lexer.Next();
                        double angle = ParseDouble(lexer, context);
                        orientation = Vector2.Normal(-angle / 180.0 * Math.PI);
                        break;

                    case "hidden":
                        lexer.Next();
                        wireInfo.IsVisible = false;
                        isSegment = false;
                        break;

                    case "dotted":
                        lexer.Next();
                        wireInfo.Options.LineType = Drawing.PathOptions.LineTypes.Dotted;
                        isSegment = false;
                        break;

                    case "dashed":
                        lexer.Next();
                        wireInfo.Options.LineType = Drawing.PathOptions.LineTypes.Dashed;
                        isSegment = false;
                        break;

                    case "arrow":
                        lexer.Next();
                        if (wireInfo.Segments.Count == 0)
                            wireInfo.Options.StartMarker = Drawing.PathOptions.MarkerTypes.Arrow;
                        else
                        {
                            wireInfo.Options.EndMarker = Drawing.PathOptions.MarkerTypes.Arrow;
                            if (!lexer.Check(TokenType.CloseBeak))
                                context.Diagnostics.Post(new TokenDiagnosticMessage(lexer.Token, SeverityLevel.Warning, "PW001", "Arrows can only appear at the start or end of a wire"));
                        }
                        isSegment = false;
                        break;

                    case "dot":
                        lexer.Next();
                        if (wireInfo.Segments.Count == 0)
                            wireInfo.Options.StartMarker = Drawing.PathOptions.MarkerTypes.Dot;
                        else
                        {
                            wireInfo.Options.EndMarker = Drawing.PathOptions.MarkerTypes.Dot;
                            if (!lexer.Check(TokenType.CloseBeak))
                                context.Diagnostics.Post(new TokenDiagnosticMessage(lexer.Token, SeverityLevel.Warning, "PW002", "Dots can only appear at the start or end of a wire"));
                        }
                        isSegment = false;
                        break;

                    case "/":
                        lexer.Next();
                        if (wireInfo.Segments.Count == 0)
                            wireInfo.Options.StartMarker = Drawing.PathOptions.MarkerTypes.Slash;
                        else
                        {
                            wireInfo.Options.EndMarker = Drawing.PathOptions.MarkerTypes.Slash;
                            if (!lexer.Check(TokenType.CloseBeak))
                                context.Diagnostics.Post(new TokenDiagnosticMessage(lexer.Token, SeverityLevel.Warning, "PW001", "Slashes can only appear at the start or end of a wire"));
                        }
                        isSegment = false;
                        break;

                    default:
                        context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.CouldNotRecognizeDirection);
                        lexer.Skip(~TokenType.Newline & ~TokenType.CloseBeak);
                        return null;
                }

                if (isSegment)
                {
                    bool isFixed = true;
                    if (lexer.Branch(TokenType.Plus))
                        isFixed = false;
                    if (lexer.Check(TokenType.Number))
                    {
                        double length = ParseDouble(lexer, context);
                        wireInfo.Segments.Add(new WireSegmentInfo(orientation, isFixed, length));
                    }
                    else
                        // Default wire segment with a minimum wire length defined by the options
                        wireInfo.Segments.Add(new WireSegmentInfo(orientation, false, context.Options.MinimumWireLength));
                }
            }

            // Closing beak
            if (!lexer.Branch(TokenType.CloseBeak))
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.BracketMismatch, ">");
            return wireInfo;
        }

        /// <summary>
        /// Strings two pins together using wires.
        /// </summary>
        /// <param name="pinToWire">The first pin.</param>
        /// <param name="wireInfo">The wire information.</param>
        /// <param name="wireToPin">The pin coming after the wire.</param>
        /// <param name="context">The parsing context.</param>
        private static void StringWiresTogether(IPin pinToWire, WireInfo wireInfo, IPin wireToPin, ParsingContext context)
        {
            // Create the wire
            string name = $"W:{++context.WireCount}";
            var wire = new Wire(name, wireInfo);
            context.Circuit.Add(wire);

            // Resolve the orientation for the pin to the wire
            if (pinToWire is IOrientedPin pin1)
                pin1.ResolveOrientation(wireInfo.Segments[0].Orientation, context.Diagnostics);
            if (wireToPin is IOrientedPin pin2)
                pin2.ResolveOrientation(-wireInfo.Segments[^1].Orientation, context.Diagnostics);

            // Make sure the start and end are tied together
            context.Circuit.Add(
                new OffsetConstraint($"{name}.start.x", pinToWire.X, wire.StartX),
                new OffsetConstraint($"{name}.start.y", pinToWire.Y, wire.StartY),
                new OffsetConstraint($"{name}.end.x", wireToPin.X, wire.EndX),
                new OffsetConstraint($"{name}.end.y", wireToPin.Y, wire.EndY));

            pinToWire.Connections++;
            wireToPin.Connections++;
        }

        /// <summary>
        /// Parses a single pin name.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The name of the pin.</returns>
        private static Token ParsePin(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.OpenIndex))
            {
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedPin);
                return default;
            }

            // Pin name
            var token = lexer.ReadWhile(~TokenType.Newline & ~TokenType.CloseIndex & ~TokenType.Whitespace);

            if (!lexer.Branch(TokenType.CloseIndex))
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.BracketMismatch, "]");
            return token;
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
                context.Diagnostics?.Post(new TokenDiagnosticMessage(lexer.StartToken, SeverityLevel.Error, "PE001", "Expected control statement type"));
                return false;
            }

            switch (typeToken.Content.ToString().ToLower())
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
                    context.Diagnostics?.Post(typeToken, ErrorCodes.CouldNotRecognizeOption, typeToken.Content.ToString());
                    return false;
            }
        }

        private static bool ParseSubcircuitDefinition(SimpleCircuitLexer lexer, ParsingContext context)
        {
            // Read the name of the subcircuit
            if (!lexer.Branch(TokenType.Word, out var nameToken))
            {
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedSubcircuit);
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
                    if (pinName.Content.Length == 0)
                        return false;
                    pin = component.Pins[pinName.Content.ToString().Trim()];
                    if (pin == null)
                        return false;
                }
                pin ??= component.Pins[component.Pins.Count - 1];
                ports.Add(pin);
            }
            if (lexer.Type == TokenType.EndOfContent)
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.UnexpectedEndOfCode);
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
            var subckt = new Subcircuit(nameToken.Content.ToString(), localContext.Circuit, ports, context.Diagnostics);
            context.Factory.Register(subckt);
            return true;
        }
        private static bool ParseSymbolDefinition(SimpleCircuitLexer lexer, ParsingContext context)
        {
            // Read the name of the subcircuit
            if (!lexer.Branch(TokenType.Word, out var nameToken))
            {
                context.Diagnostics?.Post(new TokenDiagnosticMessage(lexer.StartToken, SeverityLevel.Error, "PE001", "Expected symbol name"));
                return false;
            }
            string symbolKey = nameToken.Content.ToString();
            int line = lexer.Line;
            int column = lexer.Column;
            lexer.Skip(~TokenType.Newline);

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
                        xml.AppendLine(lexer.ReadWhile(~TokenType.Newline).Content.ToString());
                    }
                }
                else
                    xml.AppendLine(lexer.ReadWhile(~TokenType.Newline).Content.ToString());
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
                // Create a token for the XML problem
                var loc = new TextLocation(line + ex.LineNumber - 1, ex.LineNumber == 1 ? column + ex.LinePosition : ex.LinePosition);
                var token = new Token(lexer.Source, new(loc, loc), "".AsMemory());
                context.Diagnostics?.Post(token, ErrorCodes.XMLError, ex.Message);
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
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedSectionName);
                return false;
            }
            lexer.Skip(~TokenType.Newline);

            // Parse section contents
            context.Section.Push(nameToken.Content.ToString());
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
                var member = typeof(Options).GetProperty(nameToken.Content.ToString(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (member == null || !member.CanWrite)
                {
                    context.Diagnostics?.Post(nameToken, ErrorCodes.CouldNotRecognizeGlobalOption, nameToken.Content.ToString());
                    return false;
                }

                if (!lexer.Branch(TokenType.Equals))
                    context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedAssignment);

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
                    context.Diagnostics?.Post(nameToken, ErrorCodes.CouldNotRecognizeType, nameToken.Content.ToString());
            }

            if (lexer.Type != TokenType.EndOfContent && !lexer.Check(TokenType.Newline))
            {
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.UnexpectedEndOfLine);
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
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedVirtualChain);
                return false;
            }

            if (!lexer.Branch(TokenType.Word, out var directionToken))
            {
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedVirtualChainDirection);
                return false;
            }

            bool x = false, y = false;
            switch (directionToken.Content.ToString().ToLower())
            {
                case "x": x = true; break;
                case "y": y = true; break;
                case "xy":
                case "yx": x = true; y = true; break;
                default:
                    context.Diagnostics?.Post(directionToken, ErrorCodes.CouldNotRecognizeVirtualChainDirection, directionToken.Content.ToString());
                    return false;
            }

            // Parse the virtual chain
            ParseComponentChainStatement(lexer, context, (IPin pinToWire, WireInfo wireInfo, IPin wireToPin) =>
            {
                if (x)
                    StringVirtualWiresTogether(pin => pin.X, new(1, 0), pinToWire, wireInfo, wireToPin, context);
                if (y)
                    StringVirtualWiresTogether(pin => pin.Y, new(0, 1), pinToWire, wireInfo, wireToPin, context);
            });

            if (!lexer.Branch(TokenType.CloseParenthesis))
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.BracketMismatch, ")");
            return true;
        }
        private static void StringVirtualWiresTogether(Func<IPin, string> variableFunc, Vector2 normal, IPin pinToWire, WireInfo wireInfo, IPin wireToPin, ParsingContext context)
        {
            // We will go through each wire and only consider those that have an effect on the wires
            var segments = wireInfo.Segments;
            string lastNode = variableFunc(pinToWire);
            for (int i = 0; i < segments.Count; i++)
            {
                double dot = segments[i].Orientation.Dot(normal);

                // Create an intermediary point
                string node;
                if (i < segments.Count - 1)
                    node = $"virtual.{++context.VirtualCoordinateCount}";
                else
                    node = variableFunc(wireToPin);

                // Constrain these
                if (!segments[i].IsFixed && !dot.IsZero())
                {
                    if (dot > 0)
                        context.Circuit.Add(new MinimumConstraint($"virtual.constraint.{++context.VirtualCoordinateCount}", lastNode, node, Math.Abs(dot) * segments[i].Length));
                    else
                        context.Circuit.Add(new MinimumConstraint($"virtual.constraint.{++context.VirtualCoordinateCount}", node, lastNode, Math.Abs(dot) * segments[i].Length));
                }
                else
                {
                    double length = dot.IsZero() ? 0.0 : dot * segments[i].Length;
                    context.Circuit.Add(new OffsetConstraint($"virtual.constraint.{++context.VirtualCoordinateCount}", lastNode, node, length));
                }
                lastNode = node;
            }
        }

        private static bool ParsePropertyAssignmentStatement(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.Dash))
            {
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedPropertyAssignment);
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
                    context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedDot);
                    return false;
                }
                if (!lexer.Branch(TokenType.Word, out var propertyToken))
                {
                    context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedProperty);
                    return false;
                }

                // Find the property on the component
                var member = component.GetType().GetProperty(propertyToken.Content.ToString(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (member == null || !member.CanWrite)
                {
                    // Check if we can maybe resolve to a variant
                    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    component.CollectPossibleVariants(set);
                    if (!set.Contains(propertyToken.Content.ToString()))
                        context.Diagnostics?.Post(propertyToken, ErrorCodes.CouldNotFindPropertyOrVariant, propertyToken.Content.ToString(), component.Name);

                    // Treat it as a boolean
                    if (!lexer.Branch(TokenType.Equals))
                        context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedAssignment);
                    var value = ParseBoolean(lexer, context);
                    if (value == null)
                        return false;
                    if (value == true)
                        component.AddVariant(propertyToken.Content.ToString());
                    else
                        component.RemoveVariant(propertyToken.Content.ToString());
                    return true;
                }

                // Equal sign
                if (!lexer.Branch(TokenType.Equals))
                    context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedAssignment);

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
                    context.Diagnostics?.Post(propertyToken, ErrorCodes.CouldNotRecognizeType, propertyToken.Content.ToString(), component.Name);
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
                    context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedBoolean);
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
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedNumber);
                return double.NaN;
            }
            double result = double.Parse(numberToken.Content.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
            if (inverted)
                result = -result;
            return result;
        }
        private static string ParseString(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (lexer.Branch(TokenType.String, out var stringToken))
                return stringToken.Content[1..^1].ToString();
            else
            {
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.ExpectedString);
                return null;
            }
        }
    }
}
