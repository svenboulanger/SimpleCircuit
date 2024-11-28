using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
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
        public IEnumerable<string> Keys => new[] { _key };

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
        public DrawableMetadata GetMetadata(string key)
        {
            if (key == _key)
                return new DrawableMetadata(_key, $"A subcircuit of type '{_key}'.", "Subcircuit");
            return null;
        }

        /// <inheritdoc />
        public IDrawable Create(string key, string name, Options options, IDiagnosticHandler diagnostics)
            => new Instance(key, name, _circuit, _pins);

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="type">The key of the instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="definition">The graphical circuit definition.</param>
        /// <param name="pins">The ports.</param>
        private class Instance(string type, string name, GraphicalCircuit definition, IEnumerable<PinInfo> pins) : ScaledOrientedDrawable(name)
        {
            private readonly GraphicalCircuit _ckt = definition;
            private readonly List<PinInfo> _pins = pins.ToList();

            /// <inheritdoc />
            public override string Type { get; } = type.ToLower();

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        // Calculate the positions of the pins
                        Pins.Clear();

                        HashSet<string> takenNames = new(StringComparer.OrdinalIgnoreCase);
                        List<string> pinNames = [];
                        foreach (var pinInfo in _pins)
                        {
                            pinNames.Clear();

                            // Find the ports
                            var pin = pinInfo.GetOrCreate(context.Diagnostics, -1);
                            if (pin == null)
                            {
                                context.Diagnostics?.Post(pinInfo.Name, ErrorCodes.CouldNotFindPin, pinInfo.Name.Content, pinInfo.Component.Fullname);
                                return PresenceResult.GiveUp;
                            }

                            // Component name as pin name
                            if (takenNames.Add(pinInfo.Component.Source.Content.ToString()))
                                pinNames.Add(pinInfo.Component.Source.Content.ToString());

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
                        break;
                }
                return result;
            }

            /// <inheritdoc/>
            protected override void Draw(IGraphicsBuilder builder)
            {
                _ckt.Render(builder);
            }
        }
    }
}