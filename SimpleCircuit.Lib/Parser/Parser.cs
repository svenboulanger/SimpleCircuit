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
using System.Text.RegularExpressions;
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
            context.FlushActions();
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
                    context.Diagnostics?.Post(new TokenDiagnosticMessage(lexer.Token, SeverityLevel.Error, "PE001", $"Unrecognized {lexer.Content}"));
                    lexer.Next();
                    return false;
            }
        }

        private static bool ParseChainStatement(SimpleCircuitLexer lexer, ParsingContext context, Action<PinInfo, ParsingContext> singleComponent, Action<PinInfo, WireInfo, PinInfo, ParsingContext> stringTogether)
        {
            // component[pin] '<' wires '>' [pin]component[pin] '<' wires '>' ... '>' [pin]component
            var component = ParseComponent(lexer, context);
            if (component == null)
                return false;

            // Parse wires
            bool isFirst = true;
            Token pinToWire = default;
            while (lexer.Check(TokenType.OpenIndex | TokenType.OpenBeak))
            {
                // Read the starting pin
                pinToWire = default;
                if (lexer.Check(TokenType.OpenIndex))
                {
                    pinToWire = ParsePin(lexer, context);
                    if (pinToWire.Content.Length == 0)
                        return false;
                }

                // Parse the wire itself
                if (lexer.Check(TokenType.OpenBeak))
                {
                    var wireInfo = ParseWire(lexer, context);
                    if (wireInfo == null)
                        return false;

                    // Optional pin
                    Token wireToPin = default;
                    if (lexer.Check(TokenType.OpenIndex))
                    {
                        wireToPin = ParsePin(lexer, context);
                        if (wireToPin.Content.Length == 0)
                            return false;
                    }

                    // End component
                    var nextComponent = ParseComponent(lexer, context);
                    if (nextComponent == null)
                        return false;
                    stringTogether?.Invoke(new(component, pinToWire), wireInfo, new(nextComponent, wireToPin), context);
                    component = nextComponent;
                    isFirst = false;
                }
            }

            if (isFirst)
                singleComponent?.Invoke(new(component, pinToWire), context);
            return true;
        }

        /// <summary>
        /// Parses a chain of components.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <returns>Returns <c>true</c> if the statement was parsed successfully; otherwise, <c>false</c>.</returns>
        private static bool ParseComponentChainStatement(SimpleCircuitLexer lexer, ParsingContext context)
            => ParseChainStatement(lexer, context, ComponentChainSingle, ComponentChainWire);

        private static void ComponentChainSingle(PinInfo pin, ParsingContext context)
        {
            // Check the name of the component for wildcards
            if (pin.Component.Fullname.Contains('*'))
            {
                context.Diagnostics?.Post(pin.Component.Name, ErrorCodes.NoWildcardCharacter);
                return;
            }
            pin.GetOrCreate(context, 0);
        }
        private static void ComponentChainWire(PinInfo pinToWireInfo, WireInfo wireInfo, PinInfo wireToPinInfo, ParsingContext context)
        {
            if (pinToWireInfo.Component.Fullname.Contains('*'))
            {
                context.Diagnostics?.Post(pinToWireInfo.Component.Name, ErrorCodes.NoWildcardCharacter);
                return;
            }
            if (wireToPinInfo.Component.Fullname.ToString().Contains('*'))
            {
                context.Diagnostics?.Post(wireToPinInfo.Component.Name, ErrorCodes.NoWildcardCharacter);
                return;
            }

            // First create or get the first component and its pin
            var pinToWire = pinToWireInfo.GetOrCreate(context, -1);
            var wireToPin = wireToPinInfo.GetOrCreate(context, 0);

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
        /// Parses a component.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <returns>The component.</returns>
        private static ComponentInfo ParseComponent(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Check(TokenType.Word | TokenType.Times))
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedComponentName);
                return null;
            }

            // Get the full name of the component, this can include wildcards and section separators
            var nameToken = lexer.ReadWhile(TokenType.Word | TokenType.Integer | TokenType.Divide | TokenType.Times, shouldNotIncludeTrivia: true);
            string fullname = string.Join(DrawableFactoryDictionary.Separator, context.Section.Reverse().Union(new[] { nameToken.Content.ToString() }));
            var info = new ComponentInfo(nameToken, fullname);

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
                            info.Label = lexer.Content[1..^1].ToString();
                            lexer.Next();
                            break;

                        case TokenType.Dash:
                            lexer.Next();
                            if (lexer.Branch(TokenType.Word, out token))
                                info.Variants.Add(new(false, token.Content.ToString()));
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
                                info.Variants.Add(new(true, lexer.Content.ToString()));
                            lexer.Next();
                            break;

                        case TokenType.Newline:
                            break;

                        default:
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedLabelOrVariant, fullname);
                            lexer.Skip(~TokenType.CloseParenthesis & ~TokenType.Comma & ~TokenType.Newline);
                            break;
                    }
                }
                while (lexer.Branch(TokenType.Comma));

                if (!lexer.Branch(TokenType.CloseParenthesis))
                    context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.BracketMismatch, ")");
            }
            return info;
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

                    case "nojump":
                    case "nojmp":
                    case "njmp":
                        lexer.Next();
                        wireInfo.JumpOverWires = false;
                        isSegment = false;
                        break;

                    case "jump":
                    case "jmp":
                        lexer.Next();
                        wireInfo.JumpOverWires = true;
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
                    if (lexer.Check(TokenType.Number | TokenType.Integer))
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
                var component = ParseComponent(lexer, localContext)?.GetOrCreate(localContext);
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
            localContext.FlushActions();
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

            // Get the axis of the virtual chain
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

            // Virtual chains not necessarily create components, they simply try to align previously created elements along the defined axis
            // They achieve this by scheduling actions to be run at the end rather than taking effect immediately
            ParseChainStatement(lexer, context,
                (p, c) => c.SchedulePostProcess((context) => VirtualChainSingle(p, context, x, y)),
                (p2w, wi, w2p, c) => c.SchedulePostProcess((context) => VirtualChainWire(p2w, wi, w2p, c, x, y)));

            if (!lexer.Branch(TokenType.CloseParenthesis))
                context.Diagnostics?.Post(lexer.StartToken, ErrorCodes.BracketMismatch, ")");
            return true;
        }
        private static void VirtualChainSingle(PinInfo pin, ParsingContext context, bool x, bool y)
        {
            ILocatedDrawable[] components = null;
            if (pin.Component.Fullname.Contains('*'))
            {
                // Align all given pins of all named components that match the name with wildcard
                string format = $"^{pin.Component.Fullname.Replace("*", "[\\w_]+")}$";
                components = context.Circuit.OfType<ILocatedDrawable>().Where(p => Regex.IsMatch(p.Name, format)).ToArray();
            }
            else if (context.Factory.IsAnonymous(pin.Component.Fullname))
            {
                // Align all the given pins of all anonymous components within the same section
                string lead = pin.Component.Fullname + DrawableFactoryDictionary.AnonymousSeparator;
                components = context.Circuit.OfType<ILocatedDrawable>().Where(p => p.Name.StartsWith(lead)).ToArray();
            }
            if (components == null || components.Length == 0)
            {
                context.Diagnostics?.Post(pin.Component.Name, ErrorCodes.VirtualChainComponentNotFound, pin.Component.Fullname);
                return;
            }

            // Align
            var zeroWire = new WireInfo();
            zeroWire.Segments.Add(new(new(), true, 0.0));
            if (pin.Pin.Content.Length == 0)
            {
                // Align all the centers
                ILocatedPresence last = null;
                foreach (var presence in components)
                {
                    if (last != null)
                    {
                        if (x)
                            Align(new Vector2(1, 0), last.X, zeroWire, presence.X, context);
                        if (y)
                            Align(new Vector2(0, 1), last.Y, zeroWire, presence.Y, context);
                    }
                    last = presence;
                }
            }
            else
            {
                // Align all the pins
                ILocatedPresence last = null;
                foreach (var drawable in components)
                {
                    if (!drawable.Pins.TryGetValue(pin.Pin.Content.ToString(), out var next))
                    {
                        context.Diagnostics?.Post(pin.Pin, ErrorCodes.CannotFindPin, pin.Pin.Content.ToString(), drawable.Name);
                        return;
                    }
                    if (last != null)
                    {
                        if (x)
                            Align(new Vector2(1, 0), last.X, zeroWire, next.X, context);
                        if (y)
                            Align(new Vector2(0, 1), last.Y, zeroWire, next.Y, context);
                    }
                    last = next;
                }
            }
        }
        private static void VirtualChainWire(PinInfo pinToWireInfo, WireInfo wireInfo, PinInfo wireToPinInfo, ParsingContext context, bool x, bool y)
        {
            // Try to get the components
            if (!context.Circuit.ContainsKey(pinToWireInfo.Component.Fullname))
            {
                context.Diagnostics?.Post(pinToWireInfo.Component.Name, ErrorCodes.VirtualChainComponentNotFound, pinToWireInfo.Component.Fullname);
                return;
            }
            if (!context.Circuit.ContainsKey(wireToPinInfo.Component.Fullname))
            {
                context.Diagnostics?.Post(wireToPinInfo.Component.Name, ErrorCodes.VirtualChainComponentNotFound, wireToPinInfo.Component.Fullname);
                return;
            }

            // Both components exist, let's check the pins
            ILocatedPresence a, b;
            if (pinToWireInfo.Pin.Content.Length > 0)
                a = pinToWireInfo.GetOrCreate(context, 0);
            else
            {
                // No pin, let's take the center of the object if possible
                var component = pinToWireInfo.Component.GetOrCreate(context);
                if (component is ILocatedPresence located)
                    a = located;
                else if (component.Pins.Count > 0)
                    a = component.Pins[^1];
                else
                {
                    context.Diagnostics?.Post(pinToWireInfo.Component.Name, ErrorCodes.ComponentWithoutLocation, pinToWireInfo.Component.Fullname);
                    return;
                }
            }

            if (wireToPinInfo.Pin.Content.Length > 0)
                b = wireToPinInfo.GetOrCreate(context, 0);
            else
            {
                // No pin, let's take the center of the object if possible
                var component = wireToPinInfo.Component.GetOrCreate(context);
                if (component is ILocatedPresence located)
                    b = located;
                else if (component.Pins.Count > 0)
                    b = component.Pins[0];
                else
                {
                    context.Diagnostics?.Post(wireToPinInfo.Component.Name, ErrorCodes.ComponentWithoutLocation, wireToPinInfo.Component.Fullname);
                    return;
                }
            }

            // Align the located presences
            if (x)
                Align(new Vector2(1, 0), a.X, wireInfo, b.X, context);
            if (y)
                Align(new Vector2(0, 1), a.Y, wireInfo, b.Y, context);
        }
        private static void Align(Vector2 normal, string a, WireInfo wireInfo, string b, ParsingContext context)
        {
            double offset = 0;
            bool extendLeft = false, extendRight = false;

            // We will go through each wire and only consider those that have an effect on the wires
            var segments = wireInfo.Segments;
            for (int i = 0; i < segments.Count; i++)
            {
                double dot = segments[i].Orientation.Dot(normal);
                offset += dot * segments[i].Length;
                if (!dot.IsZero() && !segments[i].IsFixed)
                {
                    if (dot < 0)
                        extendLeft = true;
                    if (dot > 0)
                        extendRight = true;
                }
            }

            if (extendLeft && extendRight)
            {
                // The virtual wire can extend in both directions at some point, so this
                // virtual wire doesn't actually fix anything...
                return;
            }

            if (extendLeft)
            {
                (a, b) = (b, a);
                extendRight = true;
                offset = -offset;
            }

            if (extendRight)
            {
                // The difference is a minimum
                context.Circuit.Add(new MinimumConstraint($"virtual.constraint.{++context.VirtualCoordinateCount}", a, b, offset));
            }
            else
            {
                // Fixed offset
                context.Circuit.Add(new OffsetConstraint($"virtual.constraint.{++context.VirtualCoordinateCount}", a, b, offset));
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
                var component = ParseComponent(lexer, context)?.GetOrCreate(context);
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
            if (!lexer.Branch(TokenType.Number | TokenType.Integer, out var numberToken))
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
