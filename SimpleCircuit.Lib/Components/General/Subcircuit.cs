using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A subcircuit definition.
    /// </summary>
    public class Subcircuit : IDrawableFactory
    {
        private readonly string _key;
        private readonly GraphicalCircuit _circuit;
        private readonly IEnumerable<PinInfo> _pins;

        /// <inheritdoc />
        public IEnumerable<DrawableMetadata> Metadata
        {
            get
            {
                yield return new(new[] { _key }, $"A subcircuit of type '{_key}'", new[] { "Subcircuit" });
            }
        }

        /// <summary>
        /// Creates a new subcircuit factory.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="definition">The definition.</param>
        /// <param name="pins">The pins.</param>
        /// <param name="diagnostics">The diagnostic handler.</param>
        /// <exception cref="ArgumentNullException">Thrown if an argument is <c>null</c>.</exception>
        public Subcircuit(string key, GraphicalCircuit definition, IEnumerable<PinInfo> pins, IDiagnosticHandler diagnostics)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            _key = key;
            _circuit = definition ?? throw new ArgumentNullException(nameof(definition));
            _pins = pins ?? throw new ArgumentNullException(nameof(pins));
            if (!_circuit.Solved)
                _circuit.Solve(diagnostics);
        }

        /// <inheritdoc />
        public IDrawable Create(string key, string name, Options options, IDiagnosticHandler diagnostics)
            => new Instance(key, name, _circuit, _pins);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly GraphicalCircuit _ckt;
            private readonly List<PinInfo> _pins;

            /// <inheritdoc />
            public override string Type { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Subcircuit"/> class.
            /// </summary>
            /// <param name="type">The key of the instance.</param>
            /// <param name="name">The name.</param>
            /// <param name="definition">The graphical circuit definition.</param>
            /// <param name="pins">The ports.</param>
            public Instance(string type, string name, GraphicalCircuit definition, IEnumerable<PinInfo> pins)
                : base(name)
            {
                Type = type.ToLower();
                _ckt = definition;
                _pins = pins.ToList();
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

                // Calculate the positions of the pins
                Pins.Clear();

                HashSet<string> takenNames = new(StringComparer.OrdinalIgnoreCase);
                List<string> pinNames = new();
                foreach (var pinInfo in _pins)
                {
                    pinNames.Clear();

                    // Find the ports
                    var pin = pinInfo.GetOrCreate(context.Diagnostics, -1);
                    if (pin == null)
                    {
                        context.Diagnostics?.Post(pinInfo.Name, ErrorCodes.CouldNotFindPin, pinInfo.Name.Content, pinInfo.Component.Fullname);
                        return false;
                    }

                    // Component name as pin name
                    if (takenNames.Add(pinInfo.Component.Name.Content.ToString()))
                        pinNames.Add(pinInfo.Component.Name.Content.ToString());

                    // Shorthand notation for DIR
                    if (pin.Name.StartsWith("DIR") && pin.Name.Length > 3)
                    {
                        string name = pin.Name[3..];
                        if (takenNames.Add(name))
                            pinNames.Add(name);
                    }

                    // Deduce names
                    foreach (var name in pin.Owner.Pins.NamesOf(pin))
                        pinNames.Add($"{pin.Owner.Name}_{name}");

                    if (pin is IOrientedPin op)
                    {
                        Pins.Add(new FixedOrientedPin($"{pin.Owner.Name}_{pin.Name}", pin.Description, this, pin.Location, op.Orientation),
                            pinNames);
                    }
                    else
                    {
                        Pins.Add(new FixedPin($"{pin.Owner.Name}_{pin.Name}", pin.Description, this, pin.Location),
                            pinNames);
                    }
                }
                return true;
            }

            /// <inheritdoc/>
            protected override void Draw(SvgDrawing drawing)
            {
                _ckt.Render(drawing);
            }
        }
    }
}