using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A subcircuit definition.
    /// </summary>
    public class Subcircuit : IDrawableFactory
    {
        private readonly string _key;
        private readonly GraphicalCircuit _circuit;
        private readonly IEnumerable<IPin> _pins;

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
        public Subcircuit(string key, GraphicalCircuit definition, IEnumerable<IPin> pins, IDiagnosticHandler diagnostics)
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
        public IDrawable Create(string key, string name, Options options)
            => new Instance(key, name, _circuit, _pins);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly GraphicalCircuit _ckt;

            /// <inheritdoc />
            public override string Type { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Subcircuit"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string type, string name, GraphicalCircuit definition, IEnumerable<IPin> pins)
                : base(name)
            {
                Type = type;
                _ckt = definition;

                // Find the pins in the subcircuit
                var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<string> possibleNames = new();
                foreach (var pin in pins)
                {
                    possibleNames.Clear();

                    // Check with the name of the pin owner
                    string pinName = pin.Owner.Name;
                    if (taken.Add(pinName))
                        possibleNames.Add(pinName);

                    // Make a shorthand notation for DIR
                    if (pinName.StartsWith("DIR") && pinName.Length > 3)
                    {
                        pinName = pinName.Substring(3);
                        if (taken.Add(pinName))
                            possibleNames.Add(pinName);
                    }

                    // General names!
                    foreach (string pn in pin.Owner.Pins.NamesOf(pin))
                    {
                        pinName = $"{pin.Owner.Name}_{pn}";
                        if (taken.Add(pinName))
                            possibleNames.Add(pinName);
                    }

                    if (pin is IOrientedPin op)
                    {
                        Pins.Add(new FixedOrientedPin($"{pin.Owner.Name}_{pin.Name}", pin.Description, this, pin.Location, op.Orientation),
                            possibleNames);
                    }
                    else
                    {
                        Pins.Add(new FixedPin($"{pin.Owner.Name}_{pin.Name}", pin.Description, this, pin.Location),
                            possibleNames);
                    }
                }
            }

            /// <inheritdoc/>
            protected override void Draw(SvgDrawing drawing)
            {
                _ckt.Render(drawing);
            }
        }
    }
}