using SimpleCircuit.Circuits;
using SimpleCircuit.Components;
using SimpleCircuit.Constraints;
using SimpleCircuit.Contributors;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleCircuit
{
    /// <summary>
    /// The EasyCircuit parser.
    /// </summary>
    public class SimpleCircuitParser
    {
        private static readonly Contributor _one = new ConstantContributor(UnknownTypes.Scalar, 1.0);

        /// <summary>
        /// Gets the factory that is used by parsers to create components based on their name.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        public static ComponentFactory Factory { get; } = new ComponentFactory();

        private int _anonIndex = 0, _anonWireIndex = 0;
        private readonly Regex _doublePin = new Regex(@"^((?<name>\w+)|(?<name>\w+)\.(?<pin2>\w+)|(?<pin1>\w+)\.(?<name>\w+)\.(?<pin2>\w+))$");
        private readonly Regex _singlePin = new Regex(@"^(?<name>\w+)(\.(?<pin>\w+))?$");
        private readonly Regex _wire = new Regex(@"^((?<dir>[udrlneswtb\-])(?<length>\d*))+$");

        private class ComponentNode
        {
            public readonly IComponent Component;
            public readonly IPin PinBefore;
            public readonly IPin PinAfter;
            public ComponentNode(IComponent component, IPin pinBefore, IPin pinAfter)
            {
                Component = component ?? throw new ArgumentNullException(nameof(component));
                PinBefore = pinBefore;
                PinAfter = pinAfter;
            }
        }

        /// <summary>
        /// Parses the specified description.
        /// </summary>
        /// <param name="description">The description.</param>
        public Circuit Parse(string description)
        {
            var lines = description.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var ckt = new Circuit();
            foreach (var line in lines)
            {
                if (line[0] == '-')
                    ParseConstraint(ckt, line.Substring(1));
                else
                    ParseComponentChain(ckt, line);
            }
            return ckt;
        }

        private void ParseConstraint(Circuit ckt, string line)
        {
            var it = GetTokens(line).GetEnumerator();
            if (!it.MoveNext())
                return;

            switch (it.Current)
            {
                case "align":
                    if (!it.MoveNext())
                        return;
                    var dir = it.Current;
                    var nodes = new List<ComponentNode>();
                    while (it.MoveNext())
                    {
                        // Get the component and pin
                        var match = _singlePin.Match(it.Current);
                        if (!match.Success)
                            throw new Exception($"Could not parse component {it.Current}.");
                        var component = GetComponent(ckt, match.Groups["name"].Value);
                        IPin pin = null;
                        if (match.Groups["pin"].Success)
                            pin = component.Pins.First(p => p.Is(match.Groups["pin"].Value));
                        nodes.Add(new ComponentNode(component, null, pin));
                    }
                    switch (dir)
                    {
                        case "x": AlignX(ckt, nodes); return;
                        case "y": AlignY(ckt, nodes); return;
                        default:
                            throw new ArgumentException($"Invalid alignment {dir}");
                    }
            }
        }

        private void AlignX(Circuit ckt, IEnumerable<ComponentNode> nodes)
        {
            Contributor refX = null;
            foreach (var node in nodes)
            {
                Contributor toFix;
                if (node.PinAfter != null)
                    toFix = node.PinAfter.X;
                else
                    toFix = node.Component.Contributors.First(c => c.Type == UnknownTypes.X);

                if (refX == null)
                    refX = toFix;
                else
                {
                    if (toFix != null)
                    {
                        ckt.AddConstraint(new EqualsConstraint(refX, toFix));
                        refX = toFix;
                    }
                }
            }
        }

        private void AlignY(Circuit ckt, IEnumerable<ComponentNode> nodes)
        {
            Contributor refY = null;
            foreach (var node in nodes)
            {
                Contributor toFix;
                if (node.PinAfter != null)
                    toFix = node.PinAfter.Y;
                else
                    toFix = node.Component.Contributors.First(c => c.Type == UnknownTypes.Y);

                if (refY == null)
                    refY = toFix;
                else
                {
                    if (toFix != null)
                    {
                        ckt.AddConstraint(new EqualsConstraint(refY, toFix));
                        refY = toFix;
                    }
                }
            }
        }

        /// <summary>
        /// Parses a line.
        /// </summary>
        /// <param name="line">The line.</param>
        private void ParseComponentChain(Circuit ckt, string line)
        {
            ComponentNode last;
            var it = GetTokens(line).GetEnumerator();
            if (!it.MoveNext())
                return;

            // First get the component pin
            last = ParseComponentPin(ckt, it.Current);
            while (it.MoveNext())
            {
                // Read a wire
                var wireDescription = it.Current;

                // Read the next component
                if (!it.MoveNext())
                    return;
                var next = ParseComponentPin(ckt, it.Current);
                ParseWire(ckt, last, next, wireDescription);
                last = next;
            }
        }

        private ComponentNode ParseComponentPin(Circuit ckt, string token)
        {
            var match = _doublePin.Match(token);
            if (!match.Success)
                throw new ArgumentException($"Could not parse component token '{token}'.");

            var name = match.Groups["name"].Value;
            var component = GetComponent(ckt, name);

            // Add the component to the circuit and return the component pin
            var pin1 = match.Groups["pin1"].Success ? component.Pins.First(p => p.Is(match.Groups["pin1"].Value)) : null;
            var pin2 = match.Groups["pin2"].Success ? component.Pins.First(p => p.Is(match.Groups["pin2"].Value)) : null;
            return new ComponentNode(component, pin1, pin2);
        }

        private IComponent GetComponent(Circuit ckt, string name)
        {
            // Is it an anonymous name?
            IComponent component = null;
            string label = name;
            if (name.Length == 1)
                name = $"{name}:{++_anonIndex}";
            else
                ckt.TryGetValue(name, out component);

            if (component == null)
            {
                switch (name[0])
                {
                    case 'r':
                    case 'R':
                        component = new Resistor(name) { Label = label };
                        break;
                    case 'l':
                    case 'L':
                        component = new Inductor(name) { Label = label };
                        break;
                    case 'c':
                    case 'C':
                        component = new Capacitor(name) { Label = label };
                        break;
                    case 'i':
                    case 'I':
                        component = new CurrentSource(name) { Label = label };
                        break;
                    case 'n':
                    case 'N':
                        component = new Nmos(name) { Label = label };
                        break;
                    case 'p':
                    case 'P':
                        component = new Pmos(name) { Label = label };
                        break;
                    case 'g':
                    case 'G':
                        component = new Ground(name);
                        break;
                    case 'v':
                    case 'V':
                        component = new VoltageSource(name) { Label = label };
                        break;
                    case 'o':
                    case 'O':
                        component = new Opamp(name);
                        break;
                    case 'x':
                    case 'X':
                        component = new Point(name);
                        break;
                    case 's':
                    case 'S':
                        component = new Power(name) { Label = "VDD" };
                        break;
                    default:
                        throw new Exception($"Unrecognized component {name}.");
                }
                ckt.Add(component);
            }
            return component;
        }

        private ComponentNode ParseWire(Circuit ckt, ComponentNode start, ComponentNode end, string token)
        {
            // Let's try breaking it down into chunks
            var match = _wire.Match(token);
            if (!match.Success)
                throw new ArgumentException("Invalid wire description.");

            // Get the number of wires
            var wires = match.Groups["dir"].Captures.Count;
            if (wires == 0)
                throw new ArgumentException("Invalid wire description.");

            // Draw all wires
            IComponent nextComponent;
            IPin lastPin = start.PinAfter ?? start.Component.Pins.Last(), nextPin;
            for (var i = 0; i < wires; i++)
            {
                if (i < wires - 1)
                {
                    nextComponent = new Point($"X:{++_anonIndex}");
                    ckt.Add(nextComponent);
                    nextPin = nextComponent.Pins[0];
                }
                else
                {
                    nextPin = end.PinBefore ?? end.PinAfter ?? end.Component.Pins[0];
                }


                // Use the information to constrain the elements surrounding it
                var dir = match.Groups["dir"].Captures[i].Value;
                var l = match.Groups["length"].Captures[i].Value;
                Contributor definition = null;

                switch (dir)
                {
                    case "u":
                    case "n":
                    case "t":
                        ckt.AddConstraint(new EqualsConstraint(lastPin.Projection(new Vector2(0, -1)), _one));
                        ckt.AddConstraint(new EqualsConstraint(nextPin.Projection(new Vector2(0, 1)), _one));
                        ckt.AddConstraint(new EqualsConstraint(lastPin.X, nextPin.X));
                        definition = lastPin.Y - nextPin.Y;
                        break;
                    case "d":
                    case "b":
                    case "s":
                        ckt.AddConstraint(new EqualsConstraint(lastPin.Projection(new Vector2(0, 1)), _one));
                        ckt.AddConstraint(new EqualsConstraint(nextPin.Projection(new Vector2(0, -1)), _one));
                        ckt.AddConstraint(new EqualsConstraint(lastPin.X, nextPin.X));
                        definition = nextPin.Y - lastPin.Y;
                        break;
                    case "l":
                    case "w":
                        ckt.AddConstraint(new EqualsConstraint(lastPin.Projection(new Vector2(-1, 0)), _one));
                        ckt.AddConstraint(new EqualsConstraint(nextPin.Projection(new Vector2(1, 0)), _one));
                        ckt.AddConstraint(new EqualsConstraint(lastPin.Y, nextPin.Y));
                        definition = lastPin.X - nextPin.X;
                        break;
                    case "r":
                    case "e":
                        ckt.AddConstraint(new EqualsConstraint(lastPin.Projection(new Vector2(1, 0)), _one));
                        ckt.AddConstraint(new EqualsConstraint(nextPin.Projection(new Vector2(-1, 0)), _one));
                        ckt.AddConstraint(new EqualsConstraint(lastPin.Y, nextPin.Y));
                        definition = nextPin.X - lastPin.X;
                        break;
                }

                // Add the wire to the circuit
                var wire = new Wire($"W{++_anonWireIndex}", lastPin, nextPin, definition);
                if (l.Length > 0)
                    ckt.AddConstraint(new EqualsConstraint(wire.Length, new ConstantContributor(UnknownTypes.Length, double.Parse(l))));
                ckt.Wires.Add(wire);

                // Go to the next component
                lastPin = nextPin;
            }
            return end;
        }
        private IEnumerable<string> GetTokens(string line)
        {
            return line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Constrains two contributors together.
        /// </summary>
        /// <param name="ckt">The circuit to apply the constraint to.</param>
        /// <param name="a">The first contributor.</param>
        /// <param name="b">The second contributor.</param>
        public void Constrain(Circuit ckt, Contributor a, Contributor b)
        {
            // Cannot constrain what doesn't exist
            if (a == null || b == null)
                return;
            ckt.AddConstraint(new EqualsConstraint(a, b));
        }
    }
}
