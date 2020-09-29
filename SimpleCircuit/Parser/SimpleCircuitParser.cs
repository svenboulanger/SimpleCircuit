using SimpleCircuit.Circuits;
using SimpleCircuit.Components;
using SimpleCircuit.Functions;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;

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

        private class WireDescription
        {
            public string Direction;
            public double Length;
        }
        private class PinDescription
        {
            public Pin Before;
            public IComponent Component;
            public Pin After;
        }

        private int _anonIndex = 0, _wireIndex = 0;

        /// <summary>
        /// Parses the specified description.
        /// </summary>
        /// <param name="input">The description.</param>
        public Circuit Parse(string input)
        {
            var lexer = new SimpleCircuitLexer(input);
            if (!lexer.Next())
                return new Circuit();
            var ckt = new Circuit();
            ParseLines(lexer, ckt);
            return ckt;
        }

        private void ParseLines(SimpleCircuitLexer lexer, Circuit ckt)
        {
            do
            {
                ParseLine(lexer, ckt);
                if (!lexer.Is(TokenType.Newline) && !lexer.Is(TokenType.EndOfContent))
                    throw new ArgumentException();
                while (lexer.Is(TokenType.Newline))
                    lexer.Next();
            }
            while (!lexer.Is(TokenType.EndOfContent));
        }
        private void ParseLine(SimpleCircuitLexer lexer, Circuit ckt)
        {
            if (lexer.Is(TokenType.Dash))
                ParseEquation(lexer, ckt);
            else
                ParseChain(lexer, ckt);
        }
        private void ParseChain(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var start = ParsePin(lexer, ckt);
            while (lexer.Is(TokenType.OpenBracket))
            {
                if (start.Component is Point pts)
                    pts.Wires++;
                var wires = ParseWire(lexer);
                var end = ParseDoublePin(lexer, ckt);

                // String them together
                var lastPin = start.After ?? start.Component.Pins[start.Component.Pins.Count - 1];
                for (var i = 0; i < wires.Count; i++)
                {
                    Pin nextPin;
                    if (i < wires.Count - 1)
                    {
                        var pt = new Point("X" + (_anonIndex++));
                        ckt.Add(pt);
                        nextPin = pt.Pins[0];
                        pt.Wires += 2;
                    }
                    else
                    {
                        nextPin = end.Before ?? end.Component.Pins[0];
                        if (end.Component is Point pte)
                            pte.Wires++;
                    }

                    var wire = new Wire("W" + (_wireIndex++), lastPin, nextPin);
                    ckt.Wires.Add(wire);

                    // Add the necessary constraints
                    switch (wires[i].Direction)
                    {
                        case "u":
                        case "U":
                            ckt.Add(nextPin.X - lastPin.X); ckt.Add(lastPin.Y - nextPin.Y - wire.Length);
                            ckt.Add(lastPin.NormalX); ckt.Add(nextPin.NormalX);
                            ckt.Add(lastPin.NormalY + 1); ckt.Add(nextPin.NormalY - 1);
                            break;
                        case "d":
                        case "D":
                            ckt.Add(nextPin.X - lastPin.X); ckt.Add(nextPin.Y - lastPin.Y - wire.Length);
                            ckt.Add(lastPin.NormalX); ckt.Add(nextPin.NormalX);
                            ckt.Add(lastPin.NormalY - 1); ckt.Add(nextPin.NormalY + 1);
                            break;
                        case "l":
                        case "L":
                            ckt.Add(lastPin.X - nextPin.X - wire.Length); ckt.Add(lastPin.Y - nextPin.Y);
                            ckt.Add(lastPin.NormalX + 1); ckt.Add(nextPin.NormalX - 1);
                            ckt.Add(lastPin.NormalY); ckt.Add(nextPin.NormalY);
                            break;
                        case "r":
                        case "R":
                            ckt.Add(nextPin.X - lastPin.X - wire.Length); ckt.Add(lastPin.Y - nextPin.Y);
                            ckt.Add(lastPin.NormalX - 1); ckt.Add(nextPin.NormalX + 1);
                            ckt.Add(lastPin.NormalY); ckt.Add(nextPin.NormalY);
                            break;
                    }
                    if (wires[i].Length >= 0)
                        ckt.Add(wire.Length - wires[i].Length);

                    lastPin = nextPin;
                }
                start = end;
            }
        }
        private PinDescription ParsePin(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var result = new PinDescription();
            result.Component = ParseComponentLabel(lexer, ckt);
            if (lexer.Is(TokenType.OpenBracket, "["))
            {
                lexer.Next();
                var pin = ParseName(lexer);
                lexer.Check(TokenType.CloseBracket, "]");
                result.After = result.Component.Pins[pin];
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
                result.Before = result.Component.Pins[beforePin];
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
                    wires[wires.Count - 1].Length = double.Parse(lexer.Content);
                    lexer.Next();
                }
            }
            while (lexer.Is(TokenType.Word));
            lexer.Check(TokenType.CloseBracket, ">");
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
            throw new ArgumentException();
        }
        private void ParseEquation(SimpleCircuitLexer lexer, Circuit ckt)
        {
            if (!lexer.Is(TokenType.Dash))
                throw new ArgumentException();
            lexer.Next();

            // Add the first equation
            var a = ParseSum(lexer, ckt);
            lexer.Check(TokenType.Equals);
            var b = ParseSum(lexer, ckt);
            ckt.Add(a - b);

            while (lexer.Is(TokenType.Equals))
            {
                lexer.Next();
                a = b;
                b = ParseSum(lexer, ckt);
                ckt.Add(a - b);
            }
        }
        private Function ParseSum(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var result = ParseTerm(lexer, ckt);
            while (lexer.Is(TokenType.Plus) || lexer.Is(TokenType.Dash))
            {
                var op = lexer.Type;
                lexer.Next();
                if (op == TokenType.Plus)
                    result += ParseTerm(lexer, ckt);
                else
                    result -= ParseTerm(lexer, ckt);
            }
            return result;
        }
        private Function ParseTerm(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var result = ParseUnary(lexer, ckt);
            while (lexer.Is(TokenType.Times) || lexer.Is(TokenType.Divide))
            {
                var op = lexer.Type;
                lexer.Next();
                if (op == TokenType.Times)
                    result *= ParseUnary(lexer, ckt);
                else
                    result /= ParseUnary(lexer, ckt);
            }
            return result;
        }
        private Function ParseUnary(SimpleCircuitLexer lexer, Circuit ckt)
        {
            if (lexer.Is(TokenType.Plus))
            {
                lexer.Next();
                return ParseUnary(lexer, ckt);
            }
            if (lexer.Is(TokenType.Dash))
            {
                lexer.Next();
                return -ParseUnary(lexer, ckt);
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
        private Function ParseFactor(SimpleCircuitLexer lexer, Circuit ckt)
        {
            if (lexer.Is(TokenType.Number))
            {
                var result = double.Parse(lexer.Content);
                lexer.Next();
                return result;
            }
            else
            {
                object result = ParseComponent(lexer, ckt);
                if (result == null)
                    throw new ArgumentException();
                if (lexer.Is(TokenType.OpenBracket, "["))
                {
                    lexer.Next();
                    var pinName = ParseName(lexer);
                    if (!lexer.Is(TokenType.CloseBracket, "]"))
                        throw new ArgumentException();
                    lexer.Next();
                    result = ((IComponent)result).Pins[pinName];
                }
                lexer.Check(TokenType.Dot);
                var propertyName = ParseName(lexer);
                switch (propertyName)
                {
                    case "x":
                    case "X":
                        if (!(result is ITranslating posx))
                            throw new ArgumentException();
                        return posx.X;
                    case "y":
                    case "Y":
                        if (!(result is ITranslating posy))
                            throw new ArgumentException();
                        return posy.Y;
                    case "nx":
                    case "NX":
                        if (!(result is IRotating orx))
                            throw new ArgumentException();
                        return orx.NormalX;
                    case "ny":
                    case "NY":
                        if (!(result is IRotating ory))
                            throw new ArgumentException();
                        return ory.NormalY;
                    case "s":
                    case "S":
                        if (!(result is IScaling m))
                            throw new ArgumentException();
                        return m.Scale;
                }
                throw new ArgumentException();
            }
        }
        private string ParseName(SimpleCircuitLexer lexer)
        {
            if (!lexer.Is(TokenType.Word))
                throw new ArgumentException();
            var word = lexer.Content;
            lexer.Next();
            return word;
        }
        private string ParseLabel(SimpleCircuitLexer lexer)
        {
            if (!lexer.Is(TokenType.String))
                throw new ArgumentException();
            var text = lexer.Content.Substring(1, lexer.Content.Length - 2);
            lexer.Next();
            return text;
        }
        private IComponent ParseComponent(SimpleCircuitLexer lexer, Circuit ckt)
        {
            var name = ParseName(lexer);
            if (Factory.IsExact(name))
                name += ":" + (_anonIndex++);
            if (!ckt.TryGetValue(name, out var component))
            {
                component = Factory.Create(name);
                ckt.Add(component);
            }
            return component;
        }
    }
}