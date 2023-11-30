using SimpleCircuit.Components;
using SimpleCircuit.Components.Constraints;
using SimpleCircuit.Components.General;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Markers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            // Final check for queued anonymous points
            context.CheckQueuedPoints(context.Diagnostics);
            context.Options.Apply(context.Circuit);
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
                case TokenType.Pipe:
                    var result = ParseBoxAnnotation(lexer, context);
                    return result != null && result.Apply(context);

                case TokenType.Word:
                    return ParseComponentChainStatement(lexer, context);

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
        private static bool ParseComponentChainStatement(SimpleCircuitLexer lexer, ParsingContext context)
            => ParseChainStatement(lexer, context, ComponentChainWire, true);
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
            ParseChainStatement(lexer, context, (wi, c) => VirtualChainWire(wi, c, axis), false);
            if (!lexer.Branch(TokenType.CloseParenthesis))
            {
                context.Diagnostics?.Post(lexer.TokenWithTrivia, ErrorCodes.BracketMismatch, ")");
                lexer.Skip(~TokenType.Newline);
                return false;
            }
            return true;
        }
        private static bool ParseChainStatement(SimpleCircuitLexer lexer, ParsingContext context, Action<WireInfoList, ParsingContext> stringTogether, bool useAnnotations)
        {
            // component[pin] '<' wires '>' [pin]component[pin] '<' wires '>' ... '>' [pin]component
            var component = ParseComponent(lexer, context);
            if (component == null)
                return false;
            if (useAnnotations)
            {
                foreach (var annotation in context.Annotations)
                    annotation.Add(component);
            }

            // Parse wires
            bool isFirst = true;
            Token pinToWire = default;
            WireInfoList currentWires = null;
            while (lexer.Check(TokenType.OpenIndex | TokenType.OpenBeak | TokenType.Dash | TokenType.Arrow | TokenType.Pipe))
            {
                // Read the starting pin [ ... ]
                pinToWire = default;
                if (lexer.Check(TokenType.OpenIndex))
                {
                    pinToWire = ParsePin(lexer, context);
                    if (pinToWire.Content.Length == 0)
                        return false;
                }

                // Optionally, one can inline a box annotation here
                if (useAnnotations)
                {
                    var changes = ParseBoxAnnotation(lexer, context);
                    if (changes == null || !changes.Apply(context))
                    {
                        lexer.Skip(~TokenType.Newline);
                        return false;
                    }
                }

                // Parse the wire itself < ... >
                if (lexer.Check(TokenType.OpenBeak | TokenType.Dash | TokenType.Arrow))
                {
                    currentWires = ParseWire(lexer, context);
                    if (currentWires == null || currentWires.Count == 0)
                        return false;

                    // Fix the starting point and anonymous queued points of the wires
                    currentWires[0].PinToWire = new PinInfo(component, pinToWire);

                    // Use annotations
                    if (useAnnotations)
                    {
                        foreach (var annotation in context.Annotations)
                        {
                            foreach (var wire in currentWires)
                                annotation.Add(wire);
                        }
                    }

                    // Optionally, one can inline a box annotation start statement
                    if (useAnnotations)
                    {
                        var changes = ParseBoxAnnotation(lexer, context);
                        if (changes != null && changes.Count > 0)
                        {
                            if (lexer.Check(TokenType.OpenIndex | TokenType.OpenBeak | TokenType.Dash | TokenType.Arrow))
                            {
                                // Insert an anonymous point here before going to the next wire to
                                // make sure that the annotations are only using part of the wire
                                component = context.GetOrCreateAnonymousPoint(currentWires[0].Source);
                                if (component == null)
                                    return false;
                                currentWires[^1].WireToPin = new PinInfo(component, default);
                                foreach (var annotation in context.Annotations)
                                {
                                    foreach (var wire in currentWires)
                                        annotation.Add(wire);
                                }
                                stringTogether?.Invoke(currentWires, context);
                                if (!changes.Apply(context))
                                    return false;
                                continue;
                            }
                            if (!changes.Apply(context))
                                return false;
                        }
                    }

                    // Read the end of the wire definition
                    switch (lexer.Type)
                    {
                        case TokenType.OpenIndex:
                            // Read the ending pin [ ... ]
                            var wireToPin = ParsePin(lexer, context);
                            if (wireToPin.Content.Length == 0)
                                return false;

                            // We really need a component now that we have a pin
                            component = ParseComponent(lexer, context);
                            if (component == null)
                                return false;
                            if (useAnnotations)
                            {
                                foreach (var annotation in context.Annotations)
                                    annotation.Add(component);
                            }
                            currentWires[^1].WireToPin = new PinInfo(component, wireToPin);
                            stringTogether?.Invoke(currentWires, context);
                            break;

                        case TokenType.Word:

                            // Read the end component
                            component = ParseComponent(lexer, context);
                            if (component == null)
                                return false;
                            if (useAnnotations)
                            {
                                foreach (var annotation in context.Annotations)
                                    annotation.Add(component);
                            }
                            currentWires[^1].WireToPin = new PinInfo(component, default);
                            stringTogether?.Invoke(currentWires, context);
                            break;

                        case TokenType.EndOfContent:
                        case TokenType.Newline:

                            // End at a wire definition
                            stringTogether?.Invoke(currentWires, context);
                            break;

                        default:
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedComponentName);
                            return false;
                    }
                }
            }

            if (isFirst)
            {
                // There is no wire specified, only the pin
                var wireInfo = new WireInfoList
                {
                    { new WireInfo(default, null)
                    {
                        PinToWire = new PinInfo(component, pinToWire)
                    }, default, context }
                };
                stringTogether?.Invoke(wireInfo, context);
            }
            return true;
        }
        private static void ComponentChainWire(WireInfoList wireInfoList, ParsingContext context)
        {
            foreach (var wireInfo in wireInfoList)
            {
                // We just want to make sure here that the name isn't bogus
                if (wireInfo.PinToWire?.Component != null && wireInfo.PinToWire.Component.Fullname.Contains('*'))
                {
                    context.Diagnostics?.Post(wireInfo.PinToWire.Component.Source, ErrorCodes.NoWildcardCharacter);
                    return;
                }
                if (wireInfo.WireToPin?.Component != null && wireInfo.WireToPin.Component.Fullname.ToString().Contains('*'))
                {
                    context.Diagnostics?.Post(wireInfo.WireToPin.Component.Source, ErrorCodes.NoWildcardCharacter);
                    return;
                }
                if (wireInfo.PinToWire?.Component != null && wireInfo.PinToWire.Component.GetOrCreate(context) == null)
                    return;
                if (wireInfo.WireToPin?.Component != null && wireInfo.WireToPin.Component.GetOrCreate(context) == null)
                    return;

                // If there are no segments, skip creating the wire
                if (wireInfo.Segments == null || wireInfo.Segments.Count == 0)
                    return;

                // Create the wire
                var wire = wireInfo.GetOrCreate(context);
                if (wire == null)
                    return;
                if (wireInfo.PinToWire != null)
                    context.Circuit.Add(new PinOrientationConstraint($"{wireInfo.Fullname}.p1", wireInfo.PinToWire, -1, wireInfo.Segments[0], false));
                if (wireInfo.WireToPin != null)
                    context.Circuit.Add(new PinOrientationConstraint($"{wireInfo.Fullname}.p2", wireInfo.WireToPin, 0, wireInfo.Segments[^1], true));
            }
        }
        private static void VirtualChainWire(WireInfoList wireInfoList, ParsingContext context, Axis axis)
        {
            if (wireInfoList.Count > 1)
            {
                context.Diagnostics?.Post(wireInfoList[0].WireToPin.Name, ErrorCodes.VirtualChainAnonymousPoints);
                return;
            }
            var wireInfo = wireInfoList[0];

            if (axis == Axis.None)
                return;

            if (wireInfo.Segments != null && wireInfo.Segments.Count > 0 && wireInfo.WireToPin != null)
                context.Circuit.Add(new VirtualWire($"virtual.{context.VirtualCoordinateCount++}", wireInfo.PinToWire, wireInfo.Segments, wireInfo.WireToPin, axis));
            else
            {
                // Rules for filtering
                string filter = wireInfo.PinToWire.Component.Fullname;
                filter = filter.Replace(".", "\\.");
                if (context.Factory.IsAnonymous(filter))
                    filter += DrawableFactoryDictionary.AnonymousSeparator + ".+";
                filter = "^" + filter + "$";
                filter = filter.Replace("*", "[a-zA-Z0-9_]*");
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
                {
                    lexer.Skip(~TokenType.Newline);
                    return false;
                }

                // Property
                if (!lexer.Branch(TokenType.Dot))
                {
                    context.Diagnostics?.Post(lexer.TokenWithTrivia, ErrorCodes.ExpectedDot);
                    lexer.Skip(~TokenType.Newline);
                    return false;
                }
                if (!lexer.Branch(TokenType.Word, out var propertyToken))
                {
                    context.Diagnostics?.Post(lexer.TokenWithTrivia, ErrorCodes.ExpectedProperty);
                    return false;
                }

                // Equals
                if (!lexer.Branch(TokenType.Equals))
                {
                    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedAssignment);
                    lexer.Skip(~TokenType.Newline);
                    return false;
                }

                // The value
                object value = ParsePropertyValue(lexer, context);
                if (value == null)
                {
                    lexer.Skip(~TokenType.Newline);
                    return false;
                }
                result &= component.SetProperty(propertyToken, value, context.Diagnostics);
            }
            return result;
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
            ComponentInfo info;
            if (nameToken.Content.Span.Equals("X".AsSpan(), StringComparison.Ordinal))
                info = context.GetOrCreateAnonymousPoint(nameToken);
            else
                info = new(nameToken, context.GetFullname(nameToken.Content.ToString()));

            // Variants and properties
            if (lexer.Branch(TokenType.OpenParenthesis))
            {
                if (!ParseVariantsAndProperties(info, lexer, context))
                    lexer.Skip(~TokenType.CloseParenthesis & ~TokenType.Newline);
                if (!lexer.Branch(TokenType.CloseParenthesis))
                    context.Diagnostics?.Post(lexer.TokenWithTrivia, ErrorCodes.BracketMismatch, ")");
            }

            // Add to active annotations
            foreach (var annotation in context.Annotations)
                annotation.Add(info);
            return info;
        }
        private static WireInfoList ParseWire(SimpleCircuitLexer lexer, ParsingContext context)
        {
            if (!lexer.Check(TokenType.OpenBeak | TokenType.Dash | TokenType.Arrow))
            {
                context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedWire);
                return null;
            }
            var start = lexer.Track();

            var result = new WireInfoList();

            // Chain together multiple wire definitions
            bool isVisible = true, jumpOverWires = context.Options.JumpOverWires;
            var options = new GraphicOptions("wire");
            var markers = new List<Marker>();
            var segments = new List<WireSegmentInfo>();
            void AddWireSegment(Token token, Vector2 orientation)
            {
                var segment = new WireSegmentInfo(token)
                {
                    Orientation = orientation,
                    IsFixed = false,
                    Length = context.Options.MinimumWireLength
                };
                if (markers.Count > 0)
                {
                    if (segments.Count == 0)
                        segment.StartMarkers = markers.ToArray();
                    else
                        segments[^1].EndMarkers = markers.ToArray();
                    markers.Clear();
                }
                segments.Add(segment);
            }
            while (lexer.Check(TokenType.OpenBeak | TokenType.Dash | TokenType.Arrow))
            {
                if (lexer.Branch(TokenType.Dash, out var dashToken))
                    AddWireSegment(dashToken, new());
                else if (lexer.Check(TokenType.Arrow))
                {
                    AddWireSegment(lexer.Token, lexer.ArrowOrientation);
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
                                case "u": AddWireSegment(directionToken, new(0, -1)); break;
                                case "s":
                                case "d": AddWireSegment(directionToken, new(0, 1)); break;
                                case "w":
                                case "l": AddWireSegment(directionToken, new(-1, 0)); break;
                                case "e":
                                case "r": AddWireSegment(directionToken, new(1, 0)); break;
                                case "ne": AddWireSegment(directionToken, Vector2.Normal(-Math.PI * 0.25)); break;
                                case "nw": AddWireSegment(directionToken, Vector2.Normal(-Math.PI * 0.75)); break;
                                case "se": AddWireSegment(directionToken, Vector2.Normal(Math.PI * 0.25)); break;
                                case "sw": AddWireSegment(directionToken, Vector2.Normal(Math.PI * 0.75)); break;
                                case "a":
                                    double angle = ParseDouble(lexer, context);
                                    AddWireSegment(directionToken, Vector2.Normal(-angle * Math.PI / 180.0));
                                    break;

                                case "hidden": isVisible = false; break;
                                case "visible": isVisible = true; break;
                                case "nojump":
                                case "nojmp":
                                case "njmp": jumpOverWires = false; break;
                                case "jump":
                                case "jmp": jumpOverWires = true; break;
                                case "dotted": options.Classes.Add("dotted"); break;
                                case "dashed": options.Classes.Add("dashed"); break;

                                case "arrow": markers.Add(new Arrow()); break;
                                case "rarrow": markers.Add(new ReverseArrow()); break;
                                case "dot": markers.Add(new Dot()); break;
                                case "slash": markers.Add(new Slash()); break;
                                case "plus": markers.Add(new Plus()); break;
                                case "plusb": markers.Add(new Plus() { OppositeSide = true }); break;
                                case "minus": markers.Add(new Minus()); break;
                                case "minusb": markers.Add(new Minus() { OppositeSide = true }); break;
                                case "one": markers.Add(new ERDOne()); break;
                                case "onlyone": markers.Add(new ERDOnlyOne()); break;
                                case "many": markers.Add(new ERDMany()); break;
                                case "zeroone": markers.Add(new ERDZeroOne()); break;
                                case "onemany": markers.Add(new ERDOneMany()); break;
                                case "zeromany": markers.Add(new ERDZeroMany()); break;

                                case "X":
                                case "x":
                                    {
                                        // Add a forward anonymous point reference
                                        var component = context.CreateQueuedPoint(directionToken);

                                        // Finish the wire
                                        if (markers.Count > 0 && segments.Count > 0)
                                            segments[^1].EndMarkers = markers.ToArray();
                                        markers.Clear();

                                        // Create the wire info
                                        var subWireInfo = new WireInfo(lexer.GetTracked(start), context.GetWireFullname())
                                        {
                                            IsVisible = isVisible,
                                            JumpOverWires = jumpOverWires,
                                            RoundRadius = context.Options.RoundWires,
                                            Options = options,
                                            Segments = segments,
                                            WireToPin = new PinInfo(component, default),
                                        };
                                        subWireInfo.Simplify();
                                        result.Add(subWireInfo, directionToken, context);

                                        // Start a new wire
                                        start = lexer.Track();
                                        segments = new List<WireSegmentInfo>();
                                        markers.Clear();
                                    }
                                    break;

                                default:
                                    context.Diagnostics?.Post(directionToken, ErrorCodes.CouldNotRecognizeDirection, directionToken.Content.ToString());
                                    break;
                            }
                        }
                        else if (lexer.Branch(TokenType.Divide))
                            markers.Add(new Slash());
                        else if (lexer.Branch(TokenType.Plus))
                        {
                            double l = ParseDouble(lexer, context);
                            if (segments.Count > 0)
                            {
                                segments[^1].IsFixed = false;
                                segments[^1].Length = l;
                            }
                        }
                        else if (lexer.Check(TokenType.Number | TokenType.Integer))
                        {
                            double l = ParseDouble(lexer, context);
                            if (segments.Count > 0)
                            {
                                segments[^1].IsFixed = true;
                                segments[^1].Length = l;
                            }
                        }
                        else if (lexer.Branch(TokenType.Question))
                        {
                            if (!lexer.HasTrivia && lexer.Branch(TokenType.Question))
                            {
                                AddWireSegment(directionToken, new());
                                segments[^1].IsUnconstrained = true;
                                segments[^1].Length = 0.0;
                            }
                            else
                                AddWireSegment(directionToken, new());
                        }
                        else if (lexer.Type == TokenType.Newline || lexer.Type == TokenType.EndOfContent)
                        {
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.UnexpectedEndOfLine);
                            return null;
                        }
                        else
                        {
                            context.Diagnostics?.Post(lexer.Token, ErrorCodes.CouldNotRecognizeDirection, lexer.Token.Content);
                            lexer.Skip(~TokenType.Newline & ~TokenType.CloseBeak);
                            return null;
                        }
                    }

                    // Closing beak
                    if (!lexer.Branch(TokenType.CloseBeak))
                    {
                        context.Diagnostics?.Post(lexer.TokenWithTrivia, ErrorCodes.BracketMismatch, ">");
                        return null;
                    }

                    if (markers.Count > 0 && segments.Count > 0)
                        segments[^1].EndMarkers = markers.ToArray();
                }
            }

            // Create the wire info
            var wireInfo = new WireInfo(lexer.GetTracked(start), context.GetWireFullname())
            {
                IsVisible = isVisible,
                JumpOverWires = jumpOverWires,
                RoundRadius = context.Options.RoundWires,
                Options = options,
                Segments = segments,
            };
            wireInfo.Simplify();
            result.Add(wireInfo, default, context);
            return result;
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
                context.Diagnostics?.Post(lexer.TokenWithTrivia, ErrorCodes.BracketMismatch, "]");
            return token;
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
            var localContext = new ParsingContext(context);
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
                context.Diagnostics?.Post(nameToken, ErrorCodes.DuplicateSection, nameToken.Content);
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
                    context.Diagnostics.Post(templateToken, ErrorCodes.UnknownSectionTemplate, templateToken.Content);
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
        private static AnnotationChanges ParseBoxAnnotation(SimpleCircuitLexer lexer, ParsingContext context)
        {
            var result = new AnnotationChanges();
            while (lexer.Branch(TokenType.Pipe))
            {
                // Read the identifier of the annotation
                if (lexer.Branch(TokenType.Word, out var nameToken))
                {
                    // Notify the parsing context to start tracking component info's that are inside the annotation
                    var annotationInfo = new AnnotationInfo(nameToken, context.GetFullname(nameToken.Content.ToString()));

                    // Variants and properties
                    if (lexer.Branch(TokenType.OpenParenthesis))
                    {
                        if (!ParseVariantsAndProperties(annotationInfo, lexer, context))
                            lexer.Skip(~TokenType.CloseParenthesis & ~TokenType.Newline);
                        if (!lexer.Branch(TokenType.CloseParenthesis))
                            context.Diagnostics?.Post(lexer.TokenWithTrivia, ErrorCodes.BracketMismatch, ")");
                    }
                    else if (lexer.Type != TokenType.Pipe)
                    {
                        if (!ParseVariantsAndProperties(annotationInfo, lexer, context))
                        {
                            lexer.Skip(~TokenType.Newline & ~TokenType.Pipe);
                            return null;
                        }
                    }
                    if (!lexer.Branch(TokenType.Pipe))
                    {
                        context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedPipe);
                        lexer.Skip(~TokenType.Newline | ~TokenType.Pipe);
                        return null;
                    }
                    result.Add(new AnnotationPush(annotationInfo));
                }
                else
                {
                    // If not a name, just treat it as the end
                    var start = lexer.Track();
                    lexer.Skip(~TokenType.Newline & ~TokenType.Pipe);
                    if (!lexer.Branch(TokenType.Pipe))
                    {
                        context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedAnnotationStart);
                        return null;
                    }
                    result.Add(new AnnotationPop(lexer.GetTracked(start)));
                }
            }
            return result;
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

                // Parse depending on the type of the property
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
        private static bool ParseVariantsAndProperties(IDrawableInfo info, SimpleCircuitLexer lexer, ParsingContext context)
        {
            while(true)
            {
                // Parse in-line variants and properties
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
                        var word = lexer.Token;
                        lexer.Next();
                        if (lexer.Branch(TokenType.Equals))
                        {
                            // This is a parameter
                            object value = ParsePropertyValue(lexer, context);
                            if (value == null)
                            {
                                lexer.Skip(~TokenType.Newline & ~TokenType.CloseParenthesis);
                                break;
                            }
                            else
                                info.Properties.Add(word, value);
                        }
                        else
                            info.Variants.Add(new(true, word.Content.ToString()));
                        break;

                    default:
                        return true;
                }

                // Optional comma
                lexer.Branch(TokenType.Comma);
            }
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
                        object value = ParsePropertyValue(lexer, context);
                        if (value != null)
                            context.Options.AddDefaultProperty(key, propertyToken, value);
                        else
                        {
                            lexer.Skip(~TokenType.Newline);
                            return false;
                        }
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
        
        private static object ParsePropertyValue(SimpleCircuitLexer lexer, ParsingContext context)
        {
            // The value
            switch (lexer.Type)
            {
                case TokenType.Dash:
                case TokenType.Number:
                case TokenType.Integer:
                    return ParseDouble(lexer, context);

                case TokenType.String:
                    return ParseString(lexer, context);

                case TokenType.Word:
                    return ParseBoolean(lexer, context);

                case TokenType.OpenParenthesis:
                    lexer.Next();
                    var value1 = ParseDouble(lexer, context);
                    if (!lexer.Branch(TokenType.Comma))
                    {
                        context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedComma);
                        return null;
                    }
                    var value2 = ParseDouble(lexer, context);
                    if (!lexer.Branch(TokenType.CloseParenthesis))
                    {
                        context.Diagnostics?.Post(lexer.Token, ErrorCodes.BracketMismatch, ")");
                        return null;
                    }
                    return new Vector2(value1, value2);

                default:
                    context.Diagnostics?.Post(lexer.TokenWithTrivia, ErrorCodes.ExpectedPropertyValue);
                    lexer.Skip(~TokenType.Newline);
                    return null;
            }
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
