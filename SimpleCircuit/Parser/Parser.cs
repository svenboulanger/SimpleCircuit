using SimpleCircuit.Circuits;
using SimpleCircuit.Components;
using SimpleCircuit.Functions;
using System;
using System.Collections.Generic;

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
        /// <param name="context">The parsing context.</param>
        public static void Parse(Lexer lexer, ParsingContext context)
        {
            while (lexer.Type != TokenType.EndOfContent)
            {
                switch (lexer.Type)
                {
                    case TokenType.Word:
                        ParseComponentChain(lexer, context);
                        break;

                    case TokenType.Dash:
                        ParseEquation(lexer, context);
                        break;

                    case TokenType.Dot:
                        ParseOption(lexer, context);
                        break;

                    case TokenType.Whitespace:
                        lexer.SkipWhile(TokenType.Whitespace);
                        break;

                    case TokenType.Newline:
                    case TokenType.Comment:
                        lexer.Next();
                        break;

                    default:
                        throw new ParseException(lexer, $"Unrecognized '{lexer.Content}'");
                }
            }
        }

        /// <summary>
        /// Parses a chain of components.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        private static void ParseComponentChain(Lexer lexer, ParsingContext context)
        {
            // component[pin] < wire > [pin]component[pin] < wire > ...
            var component = ParseComponent(lexer, context);
            IPin nextPin = null;
            if (lexer.Type == TokenType.OpenIndex)
                nextPin = component.Pins[ParsePin(lexer)];
            nextPin ??= component.Pins[component.Pins.Count - 1];
            
            // Parse wires
            lexer.SkipWhile(TokenType.Whitespace);
            while (lexer.Type == TokenType.OpenBeak)
            {
                // Parse the wire
                lexer.SkipWhile(TokenType.Whitespace);
                var wire = ParseWire(lexer);

                // Parse an optional pin
                string pinName = null;
                lexer.SkipWhile(TokenType.Whitespace);
                if (lexer.Type == TokenType.OpenIndex)
                    pinName = ParsePin(lexer);

                // Parse the next component
                var nextComponent = ParseComponent(lexer, context);

                // Extract the previous pin
                IPin previousPin = null;
                if (pinName != null)
                    previousPin = nextComponent.Pins[pinName];
                if (previousPin == null)
                    previousPin = nextComponent.Pins[0];

                // String the pins together using wire segments
                StringWiresTogether(nextPin, wire, previousPin, context);

                // To next component
                component = nextComponent;
                nextPin = null;
                if (lexer.Type == TokenType.OpenIndex)
                    nextPin = component.Pins[ParsePin(lexer)];
                nextPin ??= component.Pins[component.Pins.Count - 1];
                lexer.SkipWhile(TokenType.Whitespace);
            }
        }

        /// <summary>
        /// Parses a component.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        /// <returns>The component.</returns>
        private static IComponent ParseComponent(Lexer lexer, ParsingContext context)
        {
            if (lexer.Type != TokenType.Word)
                throw new ParseException(lexer, $"Expected component name");
            var component = context.GetOrCreate(lexer.Content);
            lexer.Next();

            // Labels
            lexer.SkipWhile(TokenType.Whitespace);
            if (lexer.Type == TokenType.OpenParenthesis)
            {
                lexer.Next();
                lexer.SkipWhile(TokenType.Whitespace);

                if (lexer.Type != TokenType.String)
                    throw new ParseException(lexer, "Label string expected");
                string label = lexer.Content.Substring(1, lexer.Content.Length - 2);
                lexer.Next();
                lexer.SkipWhile(TokenType.Whitespace);
                if (lexer.Type != TokenType.CloseParenthesis)
                    throw new ParseException(lexer, "Closing parenthesis expected");
                lexer.Next();

                if (component is not ILabeled lbl)
                    throw new ParseException(lexer, $"Labeling is not possible for {component}");
                lbl.Label = label;
            }
            return component;
        }

        /// <summary>
        /// Parses a wire.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The wire information.</returns>
        private static List<WireInfo> ParseWire(Lexer lexer)
        {
            if (lexer.Type != TokenType.OpenBeak)
                throw new ParseException(lexer, "Wire description ('<') expected");
            lexer.Next();

            // Read the direction of the wire
            List<WireInfo> wires = new List<WireInfo>();
            lexer.SkipWhile(TokenType.Whitespace);
            while (lexer.Type != TokenType.CloseBeak)
            {
                // Get whether it has a bus cross
                bool isBus = false;
                if (lexer.Type == TokenType.Equals)
                {
                    isBus = true;
                    lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
                }

                // Get whether it is a bus
                bool hasBusCross = false;
                if (lexer.Type == TokenType.Divide)
                {
                    hasBusCross = true;
                    lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
                }

                // Get the direction
                string direction = lexer.Content;
                lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);

                // See if there is a length
                if (lexer.Type == TokenType.Number)
                {
                    double length = double.Parse(lexer.Content);
                    wires.Add(new WireInfo(direction[0], length, isBus, hasBusCross));
                    lexer.Next(); lexer.SkipWhile(TokenType.Whitespace);
                }
                else
                {
                    wires.Add(new WireInfo(direction[0], isBus, hasBusCross));
                }
            }

            // Closing beak
            if (lexer.Type != TokenType.CloseBeak)
                throw new ParseException(lexer, "Wire end ('>') expected");
            lexer.Next();

            return wires;
        }

        /// <summary>
        /// Strings two pins together using wires.
        /// </summary>
        /// <param name="first">The first pin.</param>
        /// <param name="wire">The wires.</param>
        /// <param name="end">The final pin.</param>
        /// <param name="context">The parsing context.</param>
        private static void StringWiresTogether(IPin first, List<WireInfo> wire, IPin end, ParsingContext context)
        {
            var last = first;
            var ckt = context.Circuit;

            // Keeping track of how many wires are connected to a single point
            if (first.Owner is Point pts)
                pts.Wires++;
            if (end.Owner is Point pte)
                pte.Wires++;

            int index = 0;
            for (int i = 0; i < wire.Count; i++)
            {
                var newWire = new Wire(last)
                {
                    IsBus = wire[i].IsBus,
                    HasBusCross = wire[i].HasBusCross
                };
                context.Circuit.Add(newWire);

                // Create a new point
                IPin next;
                if (i < wire.Count - 1)
                {
                    string pointName = $"X:{++index}";
                    while (context.Circuit.ContainsKey(pointName))
                        pointName = $"X:{++index}";
                    var point = new Point(pointName)
                    {
                        Wires = 2
                    };
                    ckt.Add(point);
                    next = point.Pins[0];
                }
                else
                {
                    next = end;
                }

                // Create a new length for our segment
                var length = new Unknown($"W{last}-{next}.l", UnknownTypes.Length);
                newWire.To(next, length);

                // Constraints - positions
                switch (wire[i].Direction)
                {
                    case 'u':
                        ckt.Add(next.X - last.X, $"align {next} with {last}");
                        ckt.Add(last.Y - next.Y - length, $"wire length {length}");
                        break;

                    case 'd':
                        ckt.Add(next.X - last.X, $"align {next} with {last}");
                        ckt.Add(next.Y - last.Y - length, $"wire length {length}");
                        break;

                    case 'l':
                        ckt.Add(last.X - next.X - length, $"wire length {length}");
                        ckt.Add(next.Y - last.Y, $"align {next} with {last}");
                        break;

                    case 'r':
                        ckt.Add(next.X - last.X - length, $"wire length {length}");
                        ckt.Add(next.Y - last.Y, $"align {next} with {last}");
                        break;
                }

                // Constraints - directions
                if (last is IRotating rLast)
                {
                    switch (wire[i].Direction)
                    {
                        case 'u':
                            ckt.Add(rLast.NormalY + 1, $"point {last} up");
                            ckt.Add(rLast.NormalX, $"point {last} up");
                            break;

                        case 'd':
                            ckt.Add(rLast.NormalY - 1, $"point {last} down");
                            ckt.Add(rLast.NormalX, $"point {last} down");
                            break;

                        case 'l':
                            ckt.Add(rLast.NormalX + 1, $"point {last} left");
                            ckt.Add(rLast.NormalY, $"point {last} left");
                            break;

                        case 'r':
                            ckt.Add(rLast.NormalX - 1, $"point {last} right");
                            ckt.Add(rLast.NormalY, $"point {last} right");
                            break;

                        case '?':
                            ckt.Add(last.X + rLast.NormalX * length - next.X, $"point {last} to {next}");
                            ckt.Add(last.Y + rLast.NormalY * length - next.Y, $"point {last} to {next}");
                            break;
                    }
                }
                if (next is IRotating rNext)
                {
                    switch (wire[i].Direction)
                    {
                        case 'u':
                            ckt.Add(rNext.NormalY - 1, $"point {last} up");
                            ckt.Add(rNext.NormalX, $"point {last} up");
                            break;

                        case 'd':
                            ckt.Add(rNext.NormalY + 1, $"point {last} down");
                            ckt.Add(rNext.NormalX, $"point {last} down");
                            break;

                        case 'l':
                            ckt.Add(rNext.NormalX - 1, $"point {last} left");
                            ckt.Add(rNext.NormalY, $"point {last} left");
                            break;

                        case 'r':
                            ckt.Add(rNext.NormalX + 1, $"point {last} right");
                            ckt.Add(rNext.NormalY, $"point {last} right");
                            break;

                        case '?':
                            ckt.Add(next.X + rNext.NormalX * length - last.X, $"point {next} to {last}");
                            ckt.Add(next.Y + rNext.NormalY * length - last.Y, $"point {next} to {last}");
                            break;
                    }
                }

                // fix the length if necessary
                if (wire[i].Length >= 0.0)
                    ckt.Add(length - wire[i].Length, $"fix {length} length");

                // Update the pin
                last = next;
            }
        }

        /// <summary>
        /// Parses a single pin name.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The name of the pin.</returns>
        private static string ParsePin(Lexer lexer)
        {
            // Opening indexer
            if (lexer.Type != TokenType.OpenIndex)
                throw new ParseException(lexer, "Expected opening index");
            lexer.Next();

            // Pin name
            lexer.SkipWhile(TokenType.Whitespace);
            if (lexer.Type != TokenType.Word)
                throw new ParseException(lexer, "Pin expected");
            string name = lexer.Content;
            lexer.Next();

            // Closing indexer
            lexer.SkipWhile(TokenType.Whitespace);
            if (lexer.Type != TokenType.CloseIndex)
                throw new ParseException(lexer, "Closing index expected");
            lexer.Next();

            return name;
        }

        /// <summary>
        /// Parses an option.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        private static void ParseOption(Lexer lexer, ParsingContext context)
        {
            if (lexer.Type != TokenType.Dot)
                throw new ParseException(lexer, "Dot expected");
            lexer.Next();
            if (lexer.Type != TokenType.Word)
                throw new ParseException(lexer, "Option expected");
            switch (lexer.Content.ToLower())
            {
                case "subckt":
                    lexer.Next();
                    lexer.SkipWhile(TokenType.Whitespace);
                    ParseSubcircuitDefinition(lexer, context);
                    break;

                case "option":
                case "options":
                    lexer.Next();
                    lexer.SkipWhile(TokenType.Whitespace);
                    ParseLocalOptions(lexer, context);
                    break;

                default:
                    throw new ParseException(lexer, $"Could not recognize '{lexer.Content}'");
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
            if (lexer.Type != TokenType.Word)
                throw new ParseException(lexer, $"Subcircuit name expected");
            string subcktName = lexer.Content;
            lexer.Next();

            // Check if the subcircuit doesn't already exist
            if (context.Definitions.Search(subcktName, out _))
                throw new ParseException(lexer, $"The subcircuit definition '{subcktName}' already exists");

            // Create a new parsing context to separate our circuit
            var localContext = new ParsingContext();
            var definition = new SubcircuitDefinition(localContext.Circuit);

            // Parse the pins
            lexer.SkipWhile(TokenType.Whitespace);
            while (lexer.Type != TokenType.Newline && lexer.Type != TokenType.EndOfContent)
            {
                // Parse the component
                if (lexer.Type != TokenType.Word)
                    throw new ParseException(lexer, $"Expected component");
                var component = localContext.GetOrCreate(lexer.Content);
                lexer.Next();

                // Find the pin
                IPin pin = null;
                if (lexer.Type == TokenType.OpenIndex)
                    pin = component.Pins[ParsePin(lexer)];
                pin ??= component.Pins[component.Pins.Count - 1];
                definition.Ports.Add(pin);

                lexer.SkipWhile(TokenType.Whitespace);
            }
            if (lexer.Type == TokenType.EndOfContent)
                throw new ParseException(lexer, "Unexpected end of context");
            lexer.Next();

            while (lexer.Type != TokenType.EndOfContent)
            {
                switch (lexer.Type)
                {
                    case TokenType.Dot:
                        lexer.Next();
                        if (lexer.Type == TokenType.Word && lexer.Content.ToLower() == "ends")
                        {
                            lexer.Next();
                            lexer.SkipWhile(~TokenType.Newline);
                            lexer.Next();
                            context.Definitions.Add(subcktName, definition);
                            return;
                        }
                        else
                            ParseOption(lexer, localContext);
                        break;

                    case TokenType.Word:
                        ParseComponentChain(lexer, localContext);
                        break;

                    case TokenType.Dash:
                        ParseEquation(lexer, localContext);
                        break;

                    case TokenType.Whitespace:
                        lexer.SkipWhile(TokenType.Whitespace);
                        break;

                    case TokenType.Newline:
                    case TokenType.Comment:
                        lexer.Next();
                        break;

                    default:
                        throw new ParseException(lexer, $"Unrecognized '{lexer.Content}'");
                }
            }
            throw new ParseException(lexer, $"Expected end of subcircuit definition");
        }

        /// <summary>
        /// Parses local options.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        private static void ParseLocalOptions(Lexer lexer, ParsingContext context)
        {

        }

        /// <summary>
        /// Parses an equation.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="context">The parsing context.</param>
        private static void ParseEquation(Lexer lexer, ParsingContext context)
        {
            if (lexer.Type != TokenType.Dash)
                throw new ParseException(lexer, $"A dash was expected");
            lexer.Next();
            var ckt = context.Circuit;

            // Get the first equation
            lexer.SkipWhile(TokenType.Whitespace);
            var lhs = ParseSum(lexer, context);

            // read equalities
            lexer.SkipWhile(TokenType.Whitespace);
            do
            {
                // Read the equals sign
                if (lexer.Type != TokenType.Equals)
                    throw new ParseException(lexer, $"Equality expected");
                lexer.Next();

                // Read the right-hand side
                lexer.SkipWhile(TokenType.Whitespace);
                var rhs = ParseSum(lexer, context);

                if (lhs is UnsolvableFunction && rhs.IsConstant)
                    lhs.Resolve(rhs.Value);
                else if (rhs is UnsolvableFunction && lhs.IsConstant)
                    rhs.Resolve(lhs.Value);
                else
                    ckt.Add(lhs - rhs, $"{lhs} = {rhs}");

                lexer.SkipWhile(TokenType.Whitespace);
            }
            while (lexer.Type == TokenType.Equals);
        }

        private static Function ParseSum(Lexer lexer, ParsingContext context)
        {
            var result = ParseTerm(lexer, context);
            lexer.SkipWhile(TokenType.Whitespace);
            while (lexer.Type == TokenType.Plus || lexer.Type == TokenType.Dash)
            {
                var op = lexer.Type;
                lexer.Next();

                // Right argument
                lexer.SkipWhile(TokenType.Whitespace);
                var right = ParseTerm(lexer, context);
                if (op == TokenType.Plus)
                    result += right;
                else
                    result -= right;
                lexer.SkipWhile(TokenType.Whitespace);
            }
            return result;
        }
        private static Function ParseTerm(Lexer lexer, ParsingContext context)
        {
            var result = ParseUnary(lexer, context);
            lexer.SkipWhile(TokenType.Whitespace);
            while (lexer.Type == TokenType.Times || lexer.Type == TokenType.Divide)
            {
                var op = lexer.Type;
                lexer.Next();

                // right argument
                lexer.SkipWhile(TokenType.Whitespace);
                var right = ParseUnary(lexer, context);
                if (op == TokenType.Times)
                    result *= right;
                else
                    result /= right;
                lexer.SkipWhile(TokenType.Whitespace);
            }
            return result;
        }
        private static Function ParseUnary(Lexer lexer, ParsingContext context)
        {
            if (lexer.Type == TokenType.Plus)
            {
                lexer.Next();
                lexer.SkipWhile(TokenType.Whitespace);
                return ParseUnary(lexer, context);
            }
            if (lexer.Type == TokenType.Dash)
            {
                lexer.Next();
                lexer.SkipWhile(TokenType.Whitespace);
                return -ParseUnary(lexer, context);
            }
            return ParseFactor(lexer, context);
        }
        private static Function ParseFactor(Lexer lexer, ParsingContext context)
        {
            if (lexer.Type == TokenType.Number)
            {
                var result = double.Parse(lexer.Content, System.Globalization.CultureInfo.InvariantCulture);
                lexer.Next();
                return result;
            }
            else if (lexer.Type == TokenType.Word)
            {
                string name = lexer.Content;
                lexer.Next();

                if (lexer.Type == TokenType.OpenParenthesis)
                {
                    // Function!
                    lexer.Next();
                    lexer.SkipWhile(TokenType.Whitespace);

                    switch (name.ToLower())
                    {
                        case "exp":
                            var arg = ParseSum(lexer, context);
                            lexer.SkipWhile(TokenType.Whitespace);
                            if (lexer.Type != TokenType.CloseParenthesis)
                                throw new ParseException(lexer, "Expected parenthesis");
                            lexer.Next();
                            return new Exp(arg);

                        case "wrap":
                            arg = ParseSum(lexer, context);
                            lexer.SkipWhile(TokenType.Whitespace);
                            if (lexer.Type != TokenType.CloseParenthesis)
                                throw new ParseException(lexer, "Expected parenthesis");
                            lexer.Next();
                            return new Wrap(arg, 360.0);

                        default:
                            throw new ParseException(lexer, $"Could not recognize function {name}");
                    }
                }
                else if (lexer.Type == TokenType.OpenIndex)
                {
                    // Component pin!
                    var component = context.GetOrCreate(name);
                    var pin = component.Pins[ParsePin(lexer)];
                    if (pin == null)
                        throw new ParseException(lexer, $"Could not find pin '{pin}'");

                    // Property
                    if (lexer.Type != TokenType.Dot)
                        throw new ParseException(lexer, $"Property expected");
                    lexer.Next();
                    if (lexer.Type != TokenType.Word)
                        throw new ParseException(lexer, "Property expected");
                    switch (lexer.Content.ToLower())
                    {
                        case "x":
                            lexer.Next();
                            return pin.X;
                        case "y":
                            lexer.Next();
                            return pin.Y;
                        case "nx":
                            if (pin is not IRotating)
                                throw new ParseException(lexer, $"Rotation is not possible for {pin}");
                            lexer.Next();
                            return ((IRotating)pin).NormalX;
                        case "ny":
                            if (pin is not IRotating)
                                throw new ParseException(lexer, $"Rotation is not possible for {pin}");
                            lexer.Next();
                            return ((IRotating)pin).NormalY;
                        case "s":
                            if (pin is not IScaling)
                                throw new ParseException(lexer, $"Scaling is not possible for {pin}");
                            lexer.Next();
                            return ((IScaling)pin).Scale;
                    }
                }
                else if (lexer.Type == TokenType.Dot)
                {
                    var component = context.GetOrCreate(name);
                    lexer.Next();
                    if (lexer.Type != TokenType.Word)
                        throw new ParseException(lexer, "Property expected");
                    switch (lexer.Content.ToLower())
                    {
                        case "x":
                            if (component is not ITranslating)
                                throw new ParseException(lexer, $"Translation is not possible for {component}");
                            lexer.Next();
                            return ((ITranslating)component).X;
                        case "y":
                            if (component is not ITranslating)
                                throw new ParseException(lexer, $"Translation is not possible for {component}");
                            lexer.Next();
                            return ((ITranslating)component).Y;
                        case "nx":
                            if (component is not IRotating)
                                throw new ParseException(lexer, $"Rotation is not possible for {component}");
                            lexer.Next();
                            return ((IRotating)component).NormalX;
                        case "ny":
                            if (component is not IRotating)
                                throw new ParseException(lexer, $"Rotation is not possible for {component}");
                            lexer.Next();
                            return ((IRotating)component).NormalY;
                        case "s":
                        case "scale":
                            if (component is not IScaling)
                                throw new ParseException(lexer, $"Scaling is not possible for {component}");
                            lexer.Next();
                            return ((IScaling)component).Scale;
                        case "w":
                        case "width":
                            if (component is not ISizeable)
                                throw new ParseException(lexer, $"Sizing is not possible for {component}");
                            lexer.Next();
                            return ((ISizeable)component).Width;
                        case "h":
                        case "height":
                            if (component is not ISizeable)
                                throw new ParseException(lexer, $"Sizing is not possible for {component}");
                            lexer.Next();
                            return ((ISizeable)component).Height;
                        default:
                            // Get a property
                            var pi = component.GetType().GetProperty(lexer.Content, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                            if (pi != null)
                            {
                                lexer.Next();
                                if (pi.PropertyType == typeof(Function) && pi.CanRead)
                                    return (Function)pi.GetValue(component);
                                if (pi.PropertyType == typeof(double))
                                {
                                    var sm = (Action<double>)pi.GetSetMethod().CreateDelegate(typeof(Action<double>), component);
                                    var gm = (Func<double>)pi.GetGetMethod().CreateDelegate(typeof(Func<double>), component);
                                    return new UnsolvableFunction(sm, gm);
                                }
                            }
                            throw new ParseException(lexer, $"Cannot recognize property {lexer.Content} of {component}");
                    }
                }
                else
                {
                    throw new ParseException(lexer, "Property expected");
                }
            }
            else if (lexer.Type == TokenType.OpenParenthesis)
            {
                lexer.Next();
                lexer.SkipWhile(TokenType.Whitespace);
                var result = ParseSum(lexer, context);

                lexer.SkipWhile(TokenType.Whitespace);
                if (lexer.Type != TokenType.CloseParenthesis)
                    throw new ParseException(lexer, $"Expected closing parenthesis");
                lexer.Next();
                return result;
            }
            throw new ParseException(lexer, $"Unexpected '{lexer.Content}'");
        }
    }
}
