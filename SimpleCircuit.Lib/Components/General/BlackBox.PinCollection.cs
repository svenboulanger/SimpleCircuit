using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    public partial class BlackBox
    {
        /// <summary>
        /// A pin-collection that makes pins as they are requested.
        /// </summary>
        protected class PinCollection : IPinCollection
        {
            private readonly Instance _parent;
            private readonly Dictionary<string, IPin> _pinsByName = new();
            private readonly List<IPin> _pinsByIndex = new();
            private readonly List<IPin> _pinsNorth = new(), _pinsWest = new(), _pinsEast = new(), _pinsSouth = new();

            /// <summary>
            /// Gets or sets the minimum horizontal spacing.
            /// </summary>
            public double MinimumHorizontalSpacing { get; set; } = 30.0;

            /// <summary>
            /// Gets or sets the minimum vertical spacing.
            /// </summary>
            public double MinimumVerticalSpacing { get; set; } = 20.0;

            public string Right { get; }

            public string Bottom { get; }

            /// <inheritdoc/>
            public IPin this[string name]
            {
                get
                {
                    if (_pinsByName.TryGetValue(name, out var pin))
                        return pin;
                    List<IPin> list = char.ToLower(name[0]) switch
                    {
                        'n' => _pinsNorth,
                        'e' => _pinsEast,
                        's' => _pinsSouth,
                        _ => _pinsWest,
                    };

                    // If the pin is the first in the list, we use a fixed pin at the origin.
                    pin = new LoosePin(name, name, _parent);
                    list.Add(pin);
                    _pinsByName.Add(name, pin);
                    _pinsByIndex.Add(pin);
                    return list[^1];
                }
            }

            /// <inheritdoc/>
            public IPin this[int index] => _pinsByIndex[index];

            /// <inheritdoc/>
            public int Count => _pinsByIndex.Count;

            /// <summary>
            /// Creates a new <see cref="PinCollection"/>.
            /// </summary>
            /// <param name="parent">the parent component.</param>
            public PinCollection(Instance parent)
            {
                _parent = parent;
                Right = $"{parent.Name}.right";
                Bottom = $"{parent.Name}.bottom";

                // This pin is not used, but only serves to avoid wires and stuff to work...
                var pin = new FixedPin("x", "x", _parent, new());
                _pinsByIndex.Add(pin);
            }

            /// <inheritdoc/>
            public IEnumerable<string> NamesOf(IPin pin)
            {
                if (pin is Pin p)
                    yield return p.Name;
            }

            /// <inheritdoc />
            public void Render(SvgDrawing drawing)
            {
                foreach (var pin in _pinsNorth)
                {
                    string name = Transform(pin.Name);
                    if (!string.IsNullOrWhiteSpace(name))
                        drawing.Text(name, pin.Location + new Vector2(0, 2), new(0, 1));
                }
                foreach (var pin in _pinsSouth)
                {

                    string name = Transform(pin.Name);
                    if (!string.IsNullOrWhiteSpace(name))
                        drawing.Text(name, pin.Location + new Vector2(0, -2), new(0, -1));
                }
                foreach (var pin in _pinsEast)
                {
                    string name = Transform(pin.Name);
                    if (!string.IsNullOrWhiteSpace(name))
                        drawing.Text(name, pin.Location + new Vector2(-2, 0), new(-1, 0));
                }
                foreach (var pin in _pinsWest)
                {
                    string name = Transform(pin.Name);
                    if (!string.IsNullOrWhiteSpace(name))
                        drawing.Text(name, pin.Location + new Vector2(2, 0), new(1, 0));
                }
            }

            private string Transform(string value)
            {
                string name = value.Substring(1);

                // If the name is between brackets, don't show the pin name
                if (name[0] == '(' && name[name.Length - 1] == ')')
                    return null;
                return name;
            }

            /// <inheritdoc />
            public void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
            {
                var ckt = context.Circuit;
                var map = context.Nodes.Shorts;
                void Apply(string name, string start, List<IPin> list, string end, Func<IPin, string> sel, double spacing)
                {
                    string lastNode = start;
                    int index = 0;
                    if (list.Count > 0)
                    {
                        foreach (var pin in list)
                            MinimumConstraint.AddMinimum(ckt, $"{name}.{index++}", lastNode, lastNode = map[sel(pin)], index == 1 ? spacing / 2 : spacing, 1);
                        MinimumConstraint.AddMinimum(ckt, $"{name}.{index}", lastNode, end, spacing / 2, 1);
                    }
                    else
                    {
                        MinimumConstraint.AddMinimum(ckt, $"{name}.{index}", lastNode, end, spacing, 1);
                    }
                }

                Apply($"{_parent.Name}.n", map[_parent.X], _pinsNorth, map[Right], p => p.X, MinimumHorizontalSpacing);
                Apply($"{_parent.Name}.s", map[_parent.X], _pinsSouth, map[Right], p => p.X, MinimumHorizontalSpacing);
                Apply($"{_parent.Name}.e", map[_parent.Y], _pinsEast, map[Bottom], p => p.Y, MinimumVerticalSpacing);
                Apply($"{_parent.Name}.w", map[_parent.Y], _pinsWest, map[Bottom], p => p.Y, MinimumVerticalSpacing);
            }

            /// <inheritdoc />
            public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
            {
                // We will group the X-coordinates and Y-coordinates of each side
                foreach (var pin in _pinsNorth)
                    context.Shorts.Group(_parent.Y, pin.Y);
                foreach (var pin in _pinsSouth)
                    context.Shorts.Group(Bottom, pin.Y);
                foreach (var pin in _pinsEast)
                    context.Shorts.Group(Right, pin.X);
                foreach (var pin in _pinsWest)
                    context.Shorts.Group(_parent.X, pin.X);
            }

            /// <inheritdoc />
            public IEnumerator<IPin> GetEnumerator() => _pinsByIndex.GetEnumerator();

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            public bool TryGetValue(string name, out IPin pin)
            {
                pin = this[name];
                return true;
            }
        }
    }
}
