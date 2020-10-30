using SimpleCircuit.Circuits;
using SimpleCircuit.Components;
using SimpleCircuit.Functions;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleCircuit
{
    /// <summary>
    /// The EasyCircuit parser.
    /// </summary>
    public class SimpleCircuitParser
    {
        /// <summary>
        /// Gets the factory that is used by parsers to create components based on their name.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        public static ComponentFactory Factory { get; } = new ComponentFactory();

        /// <summary>
        /// Occurs when a warning is issued.
        /// </summary>
        public event EventHandler<WarningEventArgs> Warning;

        private class ComponentProperty
        {
            public object Source;
            public PropertyInfo Property;
            public override string ToString() => $"Property '{Property?.Name ?? "?"}' of '{Source}'";
        }
        private class WireDescription
        {
            public string Direction;
            public double Length;
        }
        private class PinDescription
        {
            public TranslatingPin Before;
            public string BeforeName;
            public IComponent Component;
            public string ComponentName;
            public TranslatingPin After;
            public string AfterName;
        }
        private class SubcircuitDescription
        {
            public Circuit Circuit { get; set; }
            public IEnumerable<IPin> Pins { get; set; }
        }
        private int _anonIndex = 0, _wireIndex = 0;
        private readonly KeySearch<SubcircuitDescription> _subcircuits = new KeySearch<SubcircuitDescription>();

        /// <summary>
        /// Gets or sets the style for the graphics.
        /// </summary>
        /// <value>
        /// The cascading stylesheet.
        /// </value>
        public string Style { get; set; } = Circuit.DefaultStyle;

        /// <summary>
        /// Parses the specified description.
        /// </summary>
        /// <param name="input">The description.</param>
        public Circuit Parse(string input)
        {
            var lexer = new SimpleCircuitLexer(input);
            _subcircuits.Clear();
            if (!lexer.Next())
                return new Circuit();
            var ckt = new Circuit
            {
                Style = Style
            };
            ParseLines(lexer, ckt);
            return ckt;
        }

        private void ParseLines(SimpleCircuitLexer lexer, Circuit ckt)
        {
            do
            {
                ParseLine(lexer, ckt);
                if (!lexer.Is(TokenType.Newline) && !lexer.Is(TokenType.EndOfContent))
                    throw new ParseException($"Expected a new line", lexer.Line, lexer.Position);
                while (lexer.Is(TokenType.Newline))
                    lexer.Next();
            }
            while (!lexer.Is(TokenType.EndOfContent));
        }
        private void ParseLine(SimpleCircuitLexer lexer, Circuit ckt)
        {
            if (lexer.Is(TokenType.Newline))
                return;
            else if (lexer.Is(TokenType.Dash))
                ParseEquation(lexer, ckt);
            else if (lexer.Is(TokenType.Dot))
                ParseOption(lexer, ckt);
            else if (lexer.Is(TokenType.Word))
                ParseChain(lexer, ckt);
            else
                throw new ParseException($"Could not read {lexer.Content}", lexer.Line, lexer.Position);
        }
        private void ParseChain(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var start = ParsePin(lexer, ckt);
            PinDescription end = null;
            while (lexer.Is(TokenType.OpenBracket))
            {
                if (start.Component is Point pts)
                    pts.Wires++;
                var wires = ParseWire(lexer);
                end = ParseDoublePin(lexer, ckt);

                // String them together
                var lastPin = start.After ?? (TranslatingPin)start.Component.Pins.Last(p => p is TranslatingPin);
                var wire = new Wire(lastPin);
                ckt.Add(wire);
                for (var i = 0; i < wires.Count; i++)
                {
                    TranslatingPin nextPin;
                    if (i < wires.Count - 1)
                    {
                        var pt = new Point("X:" + (_anonIndex++));
                        ckt.Add(pt);
                        nextPin = (TranslatingPin)pt.Pins.First(p => p is TranslatingPin);
                        pt.Wires += 2;
                    }
                    else
                    {
                        nextPin = end.Before ?? (TranslatingPin)end.Component.Pins.First(p => p is TranslatingPin);
                        if (end.Component is Point pte)
                            pte.Wires++;
                    }

                    // Create a new segment for our wire
                    var length = new Unknown($"W{++_wireIndex}.Length", UnknownTypes.Length);
                    wire.To(nextPin, length);
                    var dir = wires[i].Direction.ToLower();

                    // Constrain the positions
                    switch (dir)
                    {
                        case "u": ckt.Add(nextPin.X - lastPin.X, $"align {nextPin} with {lastPin} (line {lexer.Line})"); ckt.Add(lastPin.Y - nextPin.Y - length, $"define wire {length} (line {lexer.Line})"); break;
                        case "d": ckt.Add(nextPin.X - lastPin.X, $"align {nextPin} with {lastPin} (line {lexer.Line})"); ckt.Add(nextPin.Y - lastPin.Y - length, $"define wire {length} (line {lexer.Line})"); break;
                        case "l": ckt.Add(lastPin.X - nextPin.X - length, $"define wire {length} (line {lexer.Line})"); ckt.Add(lastPin.Y - nextPin.Y, $"align {nextPin} and {lastPin} (line {lexer.Line})"); break;
                        case "r": ckt.Add(nextPin.X - lastPin.X - length, $"define wire {length} (line {lexer.Line})"); ckt.Add(lastPin.Y - nextPin.Y, $"align {nextPin} and {lastPin} (line {lexer.Line})"); break;
                    }

                    // Constrain the directions
                    if (lastPin is IRotating rlastPin)
                    {
                        switch (dir)
                        {
                            case "u": ckt.Add(rlastPin.NormalY + 1, $"point {rlastPin} up (line {lexer.Line})"); ckt.Add(rlastPin.NormalX, $"point {rlastPin} up (line {lexer.Line})"); break;
                            case "d": ckt.Add(rlastPin.NormalY - 1, $"point {rlastPin} down (line {lexer.Line})"); ckt.Add(rlastPin.NormalX, $"point {rlastPin} down (line {lexer.Line})"); break;
                            case "l": ckt.Add(rlastPin.NormalY, $"point {rlastPin} left (line {lexer.Line})"); ckt.Add(rlastPin.NormalX + 1, $"point {rlastPin} left (line {lexer.Line})"); break;
                            case "r": ckt.Add(rlastPin.NormalY, $"point {rlastPin} right (line {lexer.Line})"); ckt.Add(rlastPin.NormalX - 1, $"point {rlastPin} right (line {lexer.Line})"); break;
                            case "?":
                                ckt.Add(lastPin.X + rlastPin.NormalX * length - nextPin.X, $"point {rlastPin} to {nextPin} (line {lexer.Line})");
                                ckt.Add(lastPin.Y + rlastPin.NormalY * length - nextPin.Y, $"point {rlastPin} to {nextPin} (line {lexer.Line})");
                                break;
                        }
                    }
                    if (nextPin is IRotating rnextPin)
                    {
                        switch (dir)
                        {
                            case "u": ckt.Add(rnextPin.NormalY - 1, $"point {rnextPin} up (line {lexer.Line})"); ckt.Add(rnextPin.NormalX, $"point {rnextPin} up (line {lexer.Line})"); break;
                            case "d": ckt.Add(rnextPin.NormalY + 1, $"point {rnextPin} down (line {lexer.Line})"); ckt.Add(rnextPin.NormalX, $"point {rnextPin} down (line {lexer.Line})"); break;
                            case "l": ckt.Add(rnextPin.NormalY, $"point {rnextPin} left (line {lexer.Line})"); ckt.Add(rnextPin.NormalX - 1, $"point {rnextPin} left (line {lexer.Line})"); break;
                            case "r": ckt.Add(rnextPin.NormalY, $"point {rnextPin} right (line {lexer.Line})"); ckt.Add(rnextPin.NormalX + 1, $"point {rnextPin} right (line {lexer.Line})"); break;
                            case "?":
                                ckt.Add(nextPin.X + rnextPin.NormalX * length - lastPin.X, $"point {rnextPin} to {lastPin} (line {lexer.Line})");
                                ckt.Add(nextPin.Y + rnextPin.NormalY * length - lastPin.Y, $"point {rnextPin} to {lastPin} (line {lexer.Line})");
                                break;
                        }
                    }

                    // Fix the wire length if necessary
                    if (wires[i].Length >= 0)
                        ckt.Add(length - wires[i].Length, $"fix wire length {length} to {wires[i].Length} (line {lexer.Line})");

                    lastPin = nextPin;
                }
                start = end;
            }

            if (end != null && end.AfterName != null)
            {
                if (end.BeforeName == null)
                    Warn(this, new WarningEventArgs($"Pin {end.AfterName} was specified at the end of line {lexer.Line}, but it isn't used. Did you mean \"[{end.AfterName}]{end.ComponentName}\" instead of \"{end.ComponentName}[{end.AfterName}]\"?"));
                else
                    Warn(this, new WarningEventArgs($"Pin {end.AfterName} was specified at the end of line {lexer.Line}, but it isn't used."));
            }
        }
        private PinDescription ParsePin(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var result = new PinDescription();
            result.ComponentName = lexer.Content;
            result.Component = ParseComponentLabel(lexer, ckt);
            if (lexer.Is(TokenType.OpenBracket, "["))
            {
                lexer.Next();
                var pin = ParseName(lexer);
                lexer.Check(TokenType.CloseBracket, "]");
                result.AfterName = pin;
                result.After = (TranslatingPin)result.Component.Pins[pin];
            }
            return result;
        }
        private PinDescription ParseDoublePin(SimpleCircuitLexer lexer, Circuit ckt)
        {
            string beforePin = null;
            if (lexer.Is(TokenType.OpenBracket, "["))
            {
                lexer.Next();
                beforePin = ParseName(lexer);
                lexer.Check(TokenType.CloseBracket, "]");
            }
            var result = ParsePin(lexer, ckt);
            if (beforePin != null)
            {
                result.BeforeName = beforePin;
                result.Before = (TranslatingPin)result.Component.Pins[beforePin];
            }
            return result;
        }
        private IComponent ParseComponentLabel(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var component = ParseComponent(lexer, ckt);
            if (lexer.Is(TokenType.OpenBracket, "("))
            {
                lexer.Next();
                var label = ParseLabel(lexer);
                if (component is ILabeled lbl)
                    lbl.Label = label;
                lexer.Check(TokenType.CloseBracket, ")");
            }
            return component;
        }

        private List<WireDescription> ParseWire(SimpleCircuitLexer lexer)
        {
            var wires = new List<WireDescription>();
            lexer.Check(TokenType.OpenBracket, "<");
            do
            {
                var direction = ParseDirection(lexer);
                wires.Add(new WireDescription
                {
                    Direction = direction,
                    Length = -1.0
                });
                if (lexer.Is(TokenType.Number))
                {
                    wires[wires.Count - 1].Length = double.Parse(lexer.Content, System.Globalization.CultureInfo.InvariantCulture);
                    lexer.Next();
                }
            }
            while (!lexer.Is(TokenType.CloseBracket, ">"));
            lexer.Next();
            return wires;
        }
        private string ParseDirection(SimpleCircuitLexer lexer)
        {
            if (lexer.Is(TokenType.Word))
            {
                switch (lexer.Content)
                {
                    case "u":
                    case "d":
                    case "l":
                    case "r":
                        var dir = lexer.Content;
                        lexer.Next();
                        return dir;
                }
            }
            if (lexer.Is(TokenType.Question))
            {
                lexer.Next();
                return "?";
            }
            throw new ParseException($"Expected a direction", lexer.Line, lexer.Position);
        }
        private void ParseEquation(SimpleCircuitLexer lexer, Circuit ckt)
        {
            if (!lexer.Is(TokenType.Dash))
                throw new ParseException($"A dash was expected", lexer.Line, lexer.Position);
            lexer.Next();

            // Add the first equation
            var a = ParseSum(lexer, ckt);
            lexer.Check(TokenType.Equals);
            var b = ParseSum(lexer, ckt);
            if (a is Function fa && b is Function fb)
                ckt.Add(fa - fb, $"keep {fa} and {fb} equal");
            else if (a is ComponentProperty cp)
            {
                // Extract the property value if necessary
                if (b is ComponentProperty cpb)
                {
                    if (!cp.Property.CanRead)
                        throw new ParseException($"{cpb} cannot be extracted", lexer.Line, lexer.Position);
                    b = cp.Property.GetValue(cp.Source);
                }

                // Extract function values if necessary
                if (b is Function fb2 && fb2.IsFixed)
                    b = fb2.Value;

                // Set the property
                if (cp.Property.PropertyType == b.GetType())
                {
                    if (!cp.Property.CanWrite)
                        throw new ParseException($"{cp} cannot be set", lexer.Line, lexer.Position);
                    cp.Property.SetValue(cp.Source, b);
                }
                else
                    throw new ParseException($"Invalid type: cannot assign {b} to {a}", lexer.Line, lexer.Position);
            }
            else
                throw new ParseException($"Cannot assign {b} to {a}", lexer.Line, lexer.Position);

            while (lexer.Is(TokenType.Equals))
            {
                lexer.Next();
                a = b;
                b = ParseSum(lexer, ckt);
                if (a is Function fa2 && b is Function fb2)
                    ckt.Add(fa2 - fb2, $"keep {fa2} and {fb2} equal");
                else
                    throw new ParseException($"Cannot assign {a} to {b}", lexer.Line, lexer.Position);
            }
        }
        private object ParseSum(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var result = ParseTerm(lexer, ckt);
            while (lexer.Is(TokenType.Plus) || lexer.Is(TokenType.Dash))
            {
                var op = lexer.Type;
                lexer.Next();
                var sec = ParseTerm(lexer, ckt);
                if (result is Function r && sec is Function f)
                {
                    if (op == TokenType.Plus)
                        result = r + f;
                    else
                        result = r - f;
                }
                else
                    throw new ParseException($"Cannot add/subtract {sec}", lexer.Line, lexer.Position);
            }
            return result;
        }
        private object ParseTerm(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var result = ParseUnary(lexer, ckt);
            while (lexer.Is(TokenType.Times) || lexer.Is(TokenType.Divide))
            {
                var op = lexer.Type;
                lexer.Next();
                var sec = ParseUnary(lexer, ckt);
                if (result is Function r && sec is Function f)
                {
                    if (op == TokenType.Times)
                        result = r * f;
                    else
                        result = r / f;
                }
                else
                    throw new ParseException($"Cannot multiply/divide {sec}", lexer.Line, lexer.Position);
            }
            return result;
        }
        private object ParseUnary(SimpleCircuitLexer lexer, Circuit ckt)
        {
            if (lexer.Is(TokenType.Plus))
            {
                lexer.Next();
                return ParseUnary(lexer, ckt);
            }
            if (lexer.Is(TokenType.Dash))
            {
                lexer.Next();
                var arg = ParseUnary(lexer, ckt);
                if (arg is Function f)
                    return -f;
                else
                    throw new ParseException($"Cannot negate {arg}", lexer.Line, lexer.Position);
            }
            if (lexer.Is(TokenType.OpenBracket, "("))
            {
                lexer.Next();
                var result = ParseSum(lexer, ckt);
                lexer.Check(TokenType.CloseBracket, ")");
                return result;
            }
            return ParseFactor(lexer, ckt);
        }
        private object ParseFactor(SimpleCircuitLexer lexer, Circuit ckt)
        {
            if (lexer.Is(TokenType.Number))
            {
                var result = double.Parse(lexer.Content, System.Globalization.CultureInfo.InvariantCulture);
                lexer.Next();
                return (Function)result;
            }
            else if (lexer.Is(TokenType.String))
            {
                var result = lexer.Content.Substring(1, lexer.Content.Length - 2);
                lexer.Next();
                return result;
            }
            else
            {
                // Get the component
                object result = ParseComponent(lexer, ckt);
                if (result == null)
                    throw new ParseException("A component was expected", lexer.Line, lexer.Position);

                // Optional pin
                if (lexer.Is(TokenType.OpenBracket, "["))
                {
                    lexer.Next();
                    var pinName = ParseName(lexer);
                    if (!lexer.Is(TokenType.CloseBracket, "]"))
                        throw new ParseException("A closing bracket was expected", lexer.Line, lexer.Position);
                    lexer.Next();
                    result = ((IComponent)result).Pins[pinName];
                }

                // Then the property
                lexer.Check(TokenType.Dot);
                var propertyName = ParseName(lexer);

                // Detect unknowns (fixed names)
                if (result is IComponent || result is RotatingPin)
                {
                    switch (propertyName.ToLower())
                    {
                        case "x":
                            if (!(result is ITranslating posx))
                                throw new ParseException($"No translation is possible for {result}", lexer.Line, lexer.Position);
                            return posx.X;
                        case "y":
                            if (!(result is ITranslating posy))
                                throw new ParseException($"No translation is possible for {result}", lexer.Line, lexer.Position);
                            return posy.Y;
                        case "nx":
                            if (!(result is IRotating orx))
                                throw new ParseException($"No orientation is possible for {result}", lexer.Line, lexer.Position);
                            return orx.NormalX;
                        case "ny":
                            if (!(result is IRotating ory))
                                throw new ParseException($"No orientation is possible for {result}", lexer.Line, lexer.Position);
                            return ory.NormalY;
                        case "s":
                            if (!(result is IScaling m))
                                throw new ParseException($"No scaling is possible for {result}", lexer.Line, lexer.Position);
                            return m.Scale;
                    }
                }

                // Else get the description
                var pi = result.GetType().GetTypeInfo().GetProperty(propertyName);
                if (pi != null)
                {
                    // Deal with functions
                    if (pi.PropertyType == typeof(Function) && pi.CanRead)
                        return pi.GetValue(result);

                    // Just general component properties
                    return new ComponentProperty
                    {
                        Source = result,
                        Property = pi
                    };
                }
                else
                    throw new ParseException($"Cannot find property '{propertyName}' for '{result}'", lexer.Line, lexer.Position);
            }
            throw new ParseException($"Cannot read {lexer.Content}", lexer.Line, lexer.Position);
        }
        private string ParseName(SimpleCircuitLexer lexer)
        {
            if (!lexer.Is(TokenType.Word))
                throw new ParseException("A name was expected", lexer.Line, lexer.Position);
            var word = lexer.Content;
            lexer.Next();
            return word;
        }
        private string ParseLabel(SimpleCircuitLexer lexer)
        {
            if (!lexer.Is(TokenType.String))
                throw new ParseException("A label was expected", lexer.Line, lexer.Position);
            var text = lexer.Content.Substring(1, lexer.Content.Length - 2);
            lexer.Next();
            return text;
        }
        private IComponent ParseComponent(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var name = ParseName(lexer);
            IComponent component;

            // First try to find subcircuits
            var exact = _subcircuits.Search(name, out var definition);
            if (definition != null)
            {
                if (exact)
                    name += ":" + (_anonIndex++);
                if (ckt.TryGetValue(name, out component))
                    return component;

                component = new Subcircuit(name, definition.Circuit, definition.Pins);
                ckt.Add(component);
                return component;
            }

            // Get the component from the circuit or create a standard component
            if (Factory.IsExact(name))
                name += ":" + (_anonIndex++);
            if (ckt.TryGetValue(name, out component))
                return component;

            component = Factory.Create(name);
            ckt.Add(component);
            return component;
        }

        private void ParseOption(SimpleCircuitLexer lexer, Circuit ckt)
        {
            lexer.Check(TokenType.Dot);
            if (lexer.Is(TokenType.Word))
            {
                switch (lexer.Content.ToLower())
                {
                    case "subckt":
                        lexer.Next();
                        ParseSubcircuit(lexer, ckt);
                        break;

                    case "option":
                    case "options":
                        lexer.Next();
                        ParseOptions(lexer, ckt);
                        break;

                    default:
                        throw new ParseException($"Could not recognize '{lexer.Content}'", lexer.Line, lexer.Position);
                }
            }
            else
                throw new ParseException($"Could not recognized '{lexer.Content}', expected a word", lexer.Line, lexer.Position);
        }
        private void ParseSubcircuit(SimpleCircuitLexer lexer, Circuit ckt)
        {
            // Read the name
            if (!lexer.Is(TokenType.Word))
                throw new ParseException($"Expected subcircuit name", lexer.Line, lexer.Position);
            var subcktName = lexer.Content;
            lexer.Next();

            // First check if the subcircuit doesn't already exist
            if (_subcircuits.Search(subcktName, out _))
                throw new ParseException($"The subcircuit '{subcktName}' already exists", lexer.Line, lexer.Position);

            // Create our subcircuit
            var subckt = new Circuit();

            // First come pins
            var pins = new List<PinDescription>();
            while (!lexer.Is(TokenType.Newline) && !lexer.Is(TokenType.EndOfContent))
                pins.Add(ParsePin(lexer, subckt));

            // We will now begin reading a different circuit!
            while (!lexer.Is(TokenType.EndOfContent))
            {
                if (lexer.Is(TokenType.Newline))
                    lexer.Next();
                else if (lexer.Is(TokenType.Dot))
                {
                    lexer.Next();
                    if (lexer.Is(TokenType.Word, "ends") || lexer.Is(TokenType.Word, "end"))
                    {
                        lexer.Next();
                        break;
                    }
                }
                else if (lexer.Is(TokenType.Word))
                    ParseChain(lexer, subckt);
                else if (lexer.Is(TokenType.Dash))
                    ParseEquation(lexer, subckt);
                else
                    throw new ParseException($"Unrecognized '{lexer.Content}' inside the subcircuit definition", lexer.Line, lexer.Position);
            }

            // Store the subcircuit definition
            var definition = new SubcircuitDescription
            {
                Circuit = subckt,
                Pins = pins.Select(p => p.After ?? p.Component.Pins[p.Component.Pins.Count - 1])
            };
            _subcircuits.Add(subcktName, definition);
        }
        private void ParseOptions(SimpleCircuitLexer lexer, Circuit ckt)
        {
            while (!lexer.Is(TokenType.Newline) && !lexer.Is(TokenType.EndOfContent))
            {
                if (!lexer.Is(TokenType.Word))
                    throw new ParseException($"An option key was expected", lexer.Line, lexer.Position);
                var name = lexer.Content;
                lexer.Next();
                lexer.Check(TokenType.Equals);
                switch (name.ToLower())
                {
                    case "wirelength":
                        if (!lexer.Is(TokenType.Number))
                            throw new ParseException("A number was expected", lexer.Line, lexer.Position);
                        ckt.WireLength = double.Parse(lexer.Content);
                        lexer.Next();
                        break;

                    case "uppercasewidth":
                        if (!lexer.Is(TokenType.Number))
                            throw new ParseException("A number was expected", lexer.Line, lexer.Position);
                        ckt.UpperCharacterWidth = double.Parse(lexer.Content);
                        lexer.Next();
                        break;

                    case "lowercasewidth":
                        if (!lexer.Is(TokenType.Number))
                            throw new ParseException("A number was expected", lexer.Line, lexer.Position);
                        ckt.LowerCharacterWidth = double.Parse(lexer.Content);
                        lexer.Next();
                        break;

                    default:
                        throw new ParseException($"Could not read option {lexer.Content}", lexer.Line, lexer.Position);
                }
            }
        }


        /// <summary>
        /// Warn the user.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The warning arguments.</param>
        protected void Warn(object sender, WarningEventArgs args) => Warning?.Invoke(sender, args);

    }
}