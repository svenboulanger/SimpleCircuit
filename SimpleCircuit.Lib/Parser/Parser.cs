using SimpleCircuit.Components;
using SimpleCircuit.Components.Constraints;
using SimpleCircuit.Components.General;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Markers;
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
        private static readonly string[] _subcktEnd = new[] { "ends", "endsubckt" };
        private static readonly string[] _symbolEnd = new[] { "ends", "endsymbol" };
        private static readonly string[] _sectionEnd = new[] { "ends", "endsection" };

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
            if (lexer.Type == TokenType.Dot)
            {
                lexer.Next();
                return ParseControlStatement(lexer, context);
            }
            else
                return ParseNonControlStatement(lexer, context);
        }
        private static bool ParseNonControlStatement(SimpleCircuitLexer lexer, ParsingContext context)
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
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.CouldNotRecognizeStatementStart, lexer.Content.ToString());
                    lexer.Skip(~TokenType.Newline);
                    return false;
            }
        }
        private static bool ParseChainStatement(SimpleCircuitLexer lexer, ParsingContext context, Action<PinInfo, WireInfo, PinInfo, ParsingContext> stringTogether)
        {
            // component[pin] '<' wires '>' [pin]component[pin] '<' wires '>' ... '>' [pin]component
            var component = ParseComponent(lexer, context);
            if (component == null)
                return false;

            // Parse wires
            bool isFirst = true;
            Token pinToWire = default;
            while (lexer.Check(TokenType.OpenIndex | TokenType.OpenBeak | TokenType.Dash | TokenType.Arrow))
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
                if (lexer.Check(TokenType.OpenBeak | TokenType.Dash | TokenType.Arrow))
                {
                    var wireInfo = ParseWire(lexer, context);
                    if (wireInfo == null)
                        return false;

                    // Optional pin
                    Token wireToPin = default;
                    ComponentInfo nextComponent = null;
                    switch (lexer.Type)
                    {
                        case TokenType.OpenIndex:
                            wireToPin = ParsePin(lexer, context);
                            if (wireToPin.Content.Length == 0)
                                return false;

                            // We really need a component now that we have a pin
                            nextComponent = ParseComponent(lexer, context);
                            if (nextComponent == null)
                                return false;
                            stringTogether?.Invoke(new(component, pinToWire), wireInfo, new(nextComponent, wireToPin), context);
                            isFirst = false;
                            break;

                        case TokenType.Word:
                            // Optional end component
                            nextComponent = ParseComponent(lexer, context);
                            if (nextComponent == null)
                                return false;
                            stringTogether?.Invoke(new(component, pinToWire), wireInfo, new(nextComponent, wireToPin), context);
                            isFirst = false;
                            break;

                        case TokenType.EndOfContent:
                        case TokenType.Newline:
                            // End at a wire definition
                            stringTogether?.Invoke(new(component, pinToWire), wireInfo, default, context);
                            isFirst = false;
                            break;

                        default:
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedComponentName);
                            return false;
                    }
                    component = nextComponent;
                }
            }

            if (isFirst)
                stringTogether?.Invoke(new(component, pinToWire), null, default, context);
            return true;
        }
        private static bool ParseComponentChainStatement(SimpleCircuitLexer lexer, ParsingContext context)
            => ParseChainStatement(lexer, context, ComponentChainWire);
        private static void ComponentChainWire(PinInfo pinToWireInfo, WireInfo wireInfo, PinInfo wireToPinInfo, ParsingContext context)
        {
            // We just want to make sure here that the name isn't bogus
            if (pinToWireInfo.Component.Fullname.Contains('*'))
            {
                context.Diagnostics?.Post(pinToWireInfo.Component.Name, ErrorCodes.NoWildcardCharacter);
                return;
            }
            if (wireToPinInfo.Component != null && wireToPinInfo.Component.Fullname.ToString().Contains('*'))
            {
                context.Diagnostics?.Post(wireToPinInfo.Component.Name, ErrorCodes.NoWildcardCharacter);
                return;
            }

            // Create the components
            pinToWireInfo.Component.GetOrCreate(context);
            wireToPinInfo.Component?.GetOrCreate(context);

            // Create the wire
            if (wireInfo != null && wireInfo.Segments.Count > 0)
            {
                string fullname = context.GetWireFullname();
                context.Circuit.Add(new PinOrientationConstraint($"{fullname}.p1", pinToWireInfo, -1, wireInfo.Segments[0], false));
                context.Circuit.Add(new Wire(fullname, pinToWireInfo, wireInfo, wireToPinInfo));
                if (wireToPinInfo.Component != null)
                    context.Circuit.Add(new PinOrientationConstraint($"{fullname}.p2", wireToPinInfo, 0, wireInfo.Segments[^1], true));
            }
        }
        private static ComponentInfo ParseComponent(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Check(TokenType.Word | TokenType.Times))
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedComponentName);
                return null;
            }

            // Get the full name of the component, this can include wildcards and section separators
            var tracker = lexer.Track();
            lexer.Next();
            while (!lexer.HasTrivia && lexer.Branch(TokenType.Word | TokenType.Integer | TokenType.Divide | TokenType.Times));
            var nameToken = lexer.GetTracked(tracker, false);
            string fullname = context.GetFullname(nameToken.Content.ToString());
            var info = new ComponentInfo(nameToken, fullname);

            // Labels
            HashSet<string> possibleVariants = null;
            if (lexer.Branch(TokenType.OpenParenthesis))
            {
                do
                {
                    // Parse
                    switch (lexer.Type)
                    {
                        case TokenType.String:
                            info.Labels.Add(lexer.Token);
                            lexer.Next();
                            break;

                        case TokenType.Dash:
                            lexer.Next();
                            if (lexer.Branch(TokenType.Word, out Token token))
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
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.BracketMismatch, ")");
            }
            return info;
        }
        private static WireInfo ParseWire(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Check(TokenType.OpenBeak | TokenType.Dash | TokenType.Arrow))
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedWire);
                return null;
            }

            // Read the direction of the wire
            var wireInfo = new WireInfo()
            {
                JumpOverWires = context.Options.JumpOverWires
            };

            // Chain together multiple wire definitions
            Marker startMarker = null;
            while (lexer.Check(TokenType.OpenBeak | TokenType.Dash | TokenType.Arrow))
            {
                if (lexer.Branch(TokenType.Dash, out var dashToken))
                {
                    // Short-hand notation for <?>
                    wireInfo.Segments.Add(new(dashToken) { IsFixed = false, Length = context.Options.MinimumWireLength, Orientation = new(), StartMarker = startMarker });
                    startMarker = null;
                }
                else if (lexer.Check(TokenType.Arrow))
                {
                    // Short-hand notation for a directional wire segment
                    wireInfo.Segments.Add(new(lexer.Token) { IsFixed = false, Length = context.Options.MinimumWireLength, Orientation = lexer.ArrowOrientation, StartMarker = startMarker });
                    startMarker = null;
                    lexer.Next();
                }
                else if (lexer.Branch(TokenType.OpenBeak))
                {
                    while (lexer.Type != TokenType.CloseBeak)
                    {
                        // Get the direction
                        if (lexer.Branch(TokenType.Word, out var directionToken))
                        {
                            switch (directionToken.Content.ToString().ToLower())
                            {
                                case "n":
                                case "u":
                                    wireInfo.Segments.Add(new(directionToken) { Orientation = new(0, -1), IsFixed = false, Length = context.Options.MinimumWireLength, StartMarker = startMarker });
                                    startMarker = null;
                                    break;

                                case "s":
                                case "d":
                                    wireInfo.Segments.Add(new(directionToken) { Orientation = new(0, 1), IsFixed = false, Length = context.Options.MinimumWireLength, StartMarker = startMarker });
                                    startMarker = null;
                                    break;

                                case "w":
                                case "l":
                                    wireInfo.Segments.Add(new(directionToken) { Orientation = new(-1, 0), IsFixed = false, Length = context.Options.MinimumWireLength, StartMarker = startMarker });
                                    startMarker = null;
                                    break;

                                case "e":
                                case "r":
                                    wireInfo.Segments.Add(new(directionToken) { Orientation = new(1, 0), IsFixed = false, Length = context.Options.MinimumWireLength, StartMarker = startMarker });
                                    startMarker = null;
                                    break;

                                case "ne":
                                    wireInfo.Segments.Add(new(directionToken) { Orientation = Vector2.Normal(-Math.PI * 0.25), IsFixed = false, Length = context.Options.MinimumWireLength, StartMarker = startMarker });
                                    startMarker = null;
                                    break;

                                case "nw":
                                    wireInfo.Segments.Add(new(directionToken) { Orientation = Vector2.Normal(-Math.PI * 0.75), IsFixed = false, Length = context.Options.MinimumWireLength, StartMarker = startMarker });
                                    startMarker = null;
                                    break;

                                case "se":
                                    wireInfo.Segments.Add(new(directionToken) { Orientation = Vector2.Normal(Math.PI * 0.25), IsFixed = false, Length = context.Options.MinimumWireLength, StartMarker = startMarker });
                                    startMarker = null;
                                    break;

                                case "sw":
                                    wireInfo.Segments.Add(new(directionToken) { Orientation = Vector2.Normal(Math.PI * 0.75), IsFixed = false, Length = context.Options.MinimumWireLength, StartMarker = startMarker });
                                    startMarker = null;
                                    break;

                                case "a":
                                    double angle = ParseDouble(lexer, context);
                                    wireInfo.Segments.Add(new(directionToken) { Orientation = Vector2.Normal(-angle / 180.0 * Math.PI), IsFixed = false, Length = context.Options.MinimumWireLength, StartMarker = startMarker });
                                    startMarker = null;
                                    break;

                                case "hidden": wireInfo.IsVisible = false; break;

                                case "nojump":
                                case "nojmp":
                                case "njmp": wireInfo.JumpOverWires = false; break;

                                case "jump":
                                case "jmp": wireInfo.JumpOverWires = true; break;

                                case "dotted":
                                    wireInfo.Options.Classes.Add("dotted");
                                    break;

                                case "dashed":
                                    wireInfo.Options.Classes.Add("dashed");
                                    break;

                                case "arrow":
                                    if (wireInfo.Segments.Count == 0)
                                        startMarker = new Arrow();
                                    else
                                        wireInfo.Segments[^1].EndMarker = new Arrow();
                                    break;

                                case "rarrow":
                                    if (wireInfo.Segments.Count == 0)
                                        startMarker = new ReverseArrow();
                                    else
                                        wireInfo.Segments[^1].EndMarker = new ReverseArrow();
                                    break;

                                case "dot":
                                    if (wireInfo.Segments.Count == 0)
                                        startMarker = new Dot();
                                    else
                                        wireInfo.Segments[^1].EndMarker = new Dot();
                                    break;

                                case "slash":
                                    if (wireInfo.Segments.Count == 0)
                                        startMarker = new Slash();
                                    else
                                        wireInfo.Segments[^1].EndMarker = new Slash();
                                    break;

                                default:
                                    context.Diagnostics?.Post(directionToken, ErrorCodes.CouldNotRecognizeDirection, directionToken.Content.ToString());
                                    break;
                            }
                        }
                        else if (lexer.Branch(TokenType.Divide))
                        {
                            if (wireInfo.Segments.Count == 0)
                                startMarker = new Slash();
                            else
                                wireInfo.Segments[^1].EndMarker = new Slash();
                        }
                        else if (lexer.Branch(TokenType.Plus))
                        {
                            double l = ParseDouble(lexer, context);
                            if (wireInfo.Segments.Count > 0)
                            {
                                wireInfo.Segments[^1].IsFixed = false;
                                wireInfo.Segments[^1].Length = l;
                            }
                        }
                        else if (lexer.Check(TokenType.Number | TokenType.Integer))
                        {
                            double l = ParseDouble(lexer, context);
                            if (wireInfo.Segments.Count > 0)
                            {
                                wireInfo.Segments[^1].IsFixed = true;
                                wireInfo.Segments[^1].Length = l;
                            }
                        }
                        else if (lexer.Branch(TokenType.Question))
                        {
                            if (!lexer.HasTrivia && lexer.Branch(TokenType.Question))
                            {
                                // This wire does not have an orientation or length, it simply connects two points that need to be specified/resolved elsewhere
                                wireInfo.Segments.Add(new(directionToken) { IsFixed = false, Length = 0.0, Orientation = new(), IsUnconstrained = true });
                            }
                            else
                            {
                                // Don't give an orientation, and in that case the orientation should come from the pin itself!
                                wireInfo.Segments.Add(new(directionToken) { IsFixed = false, Length = context.Options.MinimumWireLength, Orientation = new() });
                            }
                            startMarker = null;
                        }
                        else if (lexer.Check(TokenType.Newline))
                        {
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.UnexpectedEndOfLine);
                            return null;
                        }
                        else
                        {
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.CouldNotRecognizeDirection, lexer.Token.Content);
                            lexer.Skip(~TokenType.Newline & ~TokenType.CloseBeak);
                        }
                    }

                    // Closing beak
                    if (!lexer.Branch(TokenType.CloseBeak))
                    {
                        context.Diagnostics?.Post(lexer.Token, ErrorCodes.BracketMismatch, ">");
                        return null;
                    }
                }
            }

            // Simplify the wire as much as possible
            wireInfo.Simplify();
            return wireInfo;
        }
        private static Token ParsePin(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.OpenIndex))
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedPin);
                return default;
            }

            // Pin name
            var token = lexer.ReadWhile(~TokenType.Newline & ~TokenType.CloseIndex & ~TokenType.Whitespace);

            if (!lexer.Branch(TokenType.CloseIndex))
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.BracketMismatch, "]");
            return token;
        }
        private static bool ParseControlStatement(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.Word, out var typeToken))
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedControlStatementType);
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

                case "variant":
                case "variants":
                    return ParseVariants(lexer, context);

                case "property":
                case "properties":
                    return ParseProperties(lexer, context);

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
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedSubcircuit);
                return false;
            }

            // Create a new parsing context to separate our circuit
            var localContext = new ParsingContext() { Diagnostics = context.Diagnostics };
            List<PinInfo> ports = new();

            // Parse the pins
            while (lexer.Check(~TokenType.Newline))
            {
                // Parse the component
                var component = ParseComponent(lexer, localContext);
                var drawable = component?.GetOrCreate(localContext);
                if (drawable == null)
                {
                    SkipToControlWord(lexer, _subcktEnd);
                    return false;
                }

                // Find the pin
                Token pinName = default;
                if (lexer.Check(TokenType.OpenIndex))
                {
                    pinName = ParsePin(lexer, localContext);
                    if (pinName.Content.Length == 0)
                    {
                        SkipToControlWord(lexer, _subcktEnd);
                        return false;
                    }
                }
                ports.Add(new(component, pinName));
            }

            // Parse the netlist contents
            while (lexer.Type != TokenType.EndOfContent)
            {
                if (lexer.Branch(TokenType.Dot))
                {
                    if (BranchControlWord(lexer, _subcktEnd))
                    {
                        lexer.Skip(~TokenType.Newline);

                        // Add a subcircuit definition to the context drawable factory
                        var subckt = new Subcircuit(nameToken.Content.ToString(), localContext.Circuit, ports, context.Diagnostics);
                        context.Factory.Register(subckt);
                        return true;
                    }
                    else if (!ParseControlStatement(lexer, localContext))
                    {
                        SkipToControlWord(lexer, _subcktEnd);
                        return false;
                    }
                }
                else if (!ParseNonControlStatement(lexer, localContext))
                {
                    SkipToControlWord(lexer, _subcktEnd);
                    return false;
                }
            }

            // Check if we reached the end of
            context.Diagnostics?.Post(lexer.Token, ErrorCodes.UnexpectedEndOfCode);
            return false;
        }
        private static bool ParseSymbolDefinition(SimpleCircuitLexer lexer, ParsingContext context)
        {
            // Read the name of the subcircuit
            if (!lexer.Branch(TokenType.Word, out var nameToken))
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedSymbolName);
                return false;
            }
            string symbolKey = nameToken.Content.ToString();
            lexer.Skip(~TokenType.Newline);
            var tracker = lexer.Track();

            // Read until a .ENDS and treat the contents as XML to be read
            Token xml;
            while (true)
            {
                if (lexer.Branch(TokenType.Dot))
                {
                    if (BranchControlWord(lexer, _symbolEnd))
                    {
                        xml = lexer.GetTracked(tracker);
                        lexer.Skip(~TokenType.Newline);
                        break;
                    }
                }
                else
                    lexer.Next();
            }

            // Parse the XML
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml($"<symbol>{Environment.NewLine}{xml.Content[..^4]}{Environment.NewLine}</symbol>");
            }
            catch (XmlException ex)
            {
                // Create a token for the XML problem
                var loc = new TextLocation(xml.Location.Line + ex.LineNumber - 1, ex.LineNumber == 1 ? xml.Location.Column + ex.LinePosition : ex.LinePosition);
                var token = new Token(lexer.Source, loc, "".AsMemory());
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
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedSectionName);
                return false;
            }
            if (context.SectionTemplates.ContainsKey(nameToken.Content.ToString()))
            {
                context.Diagnostics?.Post(ErrorCodes.DuplicateSection, nameToken.Content);
                if (lexer.Branch(TokenType.Word))
                    lexer.Skip(~TokenType.Newline);
                else
                    SkipToControlWord(lexer, _sectionEnd);
                return false;
            }

            if (lexer.Branch(TokenType.Word, out var templateToken))
            {
                // Try to use a previously defined section instead of this one
                if (!context.SectionTemplates.TryGetValue(templateToken.Content.ToString(), out var token))
                {
                    context.Diagnostics.Post(ErrorCodes.UnknownSectionTemplate, templateToken.Content);
                    lexer.Skip(~TokenType.Newline);
                    return false;
                }
                context.SectionTemplates.Add(nameToken.Content.ToString(), token);

                // Parse the template again
                context.PushSection(nameToken.Content.ToString());
                var sectionLexer = SimpleCircuitLexer.FromString(token.Content, lexer.Source, token.Location.Line);
                while (sectionLexer.Type != TokenType.EndOfContent)
                {
                    if (sectionLexer.Branch(TokenType.Dot))
                    {
                        if (BranchControlWord(sectionLexer, _sectionEnd))
                        {
                            context.PopSection();
                            return true;
                        }
                        else if (!ParseControlStatement(sectionLexer, context))
                            return false;
                    }
                    else if (!ParseNonControlStatement(sectionLexer, context))
                        return false;
                }
            }
            else
            {
                lexer.Skip(~TokenType.Newline);

                // Parse section contents
                context.PushSection(nameToken.Content.ToString());
                var tracker = lexer.Track();

                // Read the contents
                while (lexer.Type != TokenType.EndOfContent)
                {
                    if (lexer.Branch(TokenType.Dot))
                    {
                        if (BranchControlWord(lexer, _sectionEnd))
                        {
                            // End of section reached
                            lexer.Skip(~TokenType.Newline);
                            var token = lexer.GetTracked(tracker);
                            context.SectionTemplates.Add(nameToken.Content.ToString(), token);
                            context.PopSection();
                            return true;
                        }
                        else if (!ParseControlStatement(lexer, context))
                        {
                            lexer.Skip(~TokenType.Newline);
                            return false;
                        }
                    }
                    else if (!ParseNonControlStatement(lexer, context))
                    {
                        SkipToControlWord(lexer, _sectionEnd);
                        return false;
                    }
                }
            }

            // Check if we reached the end of
            context.Diagnostics?.Post(lexer.Token, ErrorCodes.UnexpectedEndOfCode);
            return false;
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
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedAssignment);

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
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.UnexpectedEndOfLine);
                return false;
            }
            return true;
        }
        private static bool ParseVariants(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.Word, out var tokenKey))
            {
                context.Diagnostics?.Post(tokenKey, ErrorCodes.ExpectedComponentKey);
                return false;
            }

            // Check whether the key is actually a key
            string key = tokenKey.Content.ToString();
            if (!context.Factory.IsKey(key))
            {
                context.Diagnostics?.Post(tokenKey, ErrorCodes.NotAKey, tokenKey.Content.ToString());
                return false;
            }

            // Start reading variants
            while (lexer.Check(TokenType.Word | TokenType.Comma | TokenType.Dash))
            {
                switch (lexer.Type)
                {
                    case TokenType.Word:
                        // Add the variant
                        context.Options.AddInclude(key, lexer.Content.ToString());
                        lexer.Next();
                        break;

                    case TokenType.Dash:
                        // Remove the variant
                        lexer.Next();
                        if (!lexer.Branch(TokenType.Word, out var noVariant))
                        {
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedVariant);
                            return false;
                        }
                        context.Options.AddExclude(key, noVariant.Content.ToString());
                        break;

                    case TokenType.Comma:
                        lexer.Next();
                        break;

                    default:
                        lexer.Next();
                        break;
                }
            }

            return true;
        }
        private static bool ParseProperties(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.Word, out var tokenKey))
            {
                context.Diagnostics?.Post(tokenKey, ErrorCodes.ExpectedComponentKey);
                return false;
            }

            // Check whether the key is actually a key
            string key = tokenKey.Content.ToString();
            if (!context.Factory.IsKey(key))
            {
                context.Diagnostics?.Post(tokenKey, ErrorCodes.NotAKey, tokenKey.Content.ToString());
                return false;
            }

            // Start reading properties
            while (lexer.Check(TokenType.Word))
            {
                switch (lexer.Type)
                {
                    case TokenType.Word:
                        lexer.Branch(TokenType.Word, out var propertyToken);
                        if (!lexer.Branch(TokenType.Equals))
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedPropertyAssignment);
                        object value = null;
                        switch (lexer.Type)
                        {
                            case TokenType.String:
                                value = ParseString(lexer, context);
                                break;

                            case TokenType.Word:
                                value = ParseBoolean(lexer, context);
                                break;

                            case TokenType.Integer:
                            case TokenType.Number:
                                value = ParseDouble(lexer, context);
                                break;

                            default:
                                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedPropertyValue);
                                break;
                        }
                        if (value != null)
                            context.Options.AddDefaultProperty(key, propertyToken, value);
                        break;

                    case TokenType.Comma:
                        lexer.Next();
                        break;

                    default:
                        lexer.Next();
                        break;
                }
            }
            return true;
        }
        private static bool ParseVirtualChainStatement(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.OpenParenthesis))
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedVirtualChain);
                return false;
            }

            // Determine the virtual wire alignment
            Axis axis = Axis.XY;
            if (lexer.Branch(TokenType.Word, "x"))
                axis = Axis.X;
            else if (lexer.Branch(TokenType.Word, "y"))
                axis = Axis.Y;
            else if (lexer.Branch(TokenType.Word, "xy") ||
                lexer.Branch(TokenType.Word, "yx"))
                axis = Axis.XY;

            // Virtual chains not necessarily create components, they simply try to align previously created elements along the defined axis
            // They achieve this by scheduling actions to be run at the end rather than taking effect immediately
            ParseChainStatement(lexer, context,
                (p2w, wi, w2p, c) => VirtualChainWire(p2w, wi, w2p, c, axis));

            if (!lexer.Branch(TokenType.CloseParenthesis))
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.BracketMismatch, ")");
            return true;
        }

        private static void VirtualChainWire(PinInfo pinToWireInfo, WireInfo wireInfo, PinInfo wireToPinInfo, ParsingContext context, Axis axis)
        {
            if (axis == Axis.None)
                return;

            if (wireInfo != null)
                context.Circuit.Add(new VirtualWire($"virtual.{context.VirtualCoordinateCount++}", pinToWireInfo, wireInfo, wireToPinInfo, axis));
            else
            {
                // Rules for filtering
                string filter = pinToWireInfo.Component.Fullname;
                filter = filter.Replace(".", "\\.");
                if (context.Factory.IsAnonymous(filter))
                    filter += DrawableFactoryDictionary.AnonymousSeparator + ".+";
                filter = "^" + filter + "$";
                filter = filter.Replace("*", "[a-zA-Z0-9_]*");
                Console.WriteLine(filter);
                var regex = new Regex(filter, RegexOptions.IgnoreCase);
                var presences = context.Circuit.OfType<ILocatedPresence>().Where(p => regex.IsMatch(p.Name));
                context.Circuit.Add(new AlignedWire($"virtual.{context.VirtualCoordinateCount++}", presences, axis));
            }
        }
        private static bool ParsePropertyAssignmentStatement(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.Dash))
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedPropertyAssignment);
                return false;
            }

            bool result = true;
            while (lexer.Check(~TokenType.Newline))
            {
                // Parse the component
                var component = ParseComponent(lexer, context)?.GetOrCreate(context);
                if (component == null)
                    return false;

                // Property
                if (!lexer.Branch(TokenType.Dot))
                {
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedDot);
                    return false;
                }
                if (!lexer.Branch(TokenType.Word, out var propertyToken))
                {
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedProperty);
                    return false;
                }

                // Equals
                if (!lexer.Branch(TokenType.Equals))
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedAssignment);

                // The value
                object value = null;
                switch (lexer.Type)
                {
                    case TokenType.Number:
                    case TokenType.Integer:
                        value = ParseDouble(lexer, context);
                        break;

                    case TokenType.String:
                        value = ParseString(lexer, context);
                        break;

                    case TokenType.Word:
                        value = ParseBoolean(lexer, context);
                        break;

                    default:
                        context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedPropertyValue);
                        return false;
                }
                result &= context.Options.SetProperty(context.Diagnostics, component, propertyToken, value);
            }
            return result;
        }
        private static bool? ParseBoolean(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Branch(TokenType.Word, "true", StringComparison.Ordinal))
            {
                if (!lexer.Branch(TokenType.Word, "false", StringComparison.Ordinal))
                {
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedBoolean);
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
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedNumber);
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
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedString);
                return null;
            }
        }

        private static bool BranchControlWord(SimpleCircuitLexer lexer, params string[] names)
        {
            if (lexer.HasTrivia)
                return false;
            for (int i = 0; i < names.Length; i++)
            {
                if (lexer.Branch(TokenType.Word, names[i]))
                    return true;
            }
            return false;
        }
        private static void SkipToControlWord(SimpleCircuitLexer lexer, params string[] names)
        {
            bool keepGoing = true;
            while (keepGoing)
            {
                while (lexer.Check(~TokenType.Dot))
                    lexer.Next();
                if (lexer.Branch(TokenType.Dot))
                {
                    if (BranchControlWord(lexer, names))
                        return;
                }
                else
                    keepGoing = false;
            }
        }
    }
}
