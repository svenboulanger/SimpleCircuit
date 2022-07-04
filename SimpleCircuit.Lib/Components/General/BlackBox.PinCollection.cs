using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

            /// <summary>
            /// Gets or sets the minimum horizontal spacing.
            /// </summary>
            public double MinimumHorizontalSpacing { get; set; } = 30.0;

            /// <summary>
            /// Gets or sets the minimum vertical spacing.
            /// </summary>
            public double MinimumVerticalSpacing { get; set; } = 20.0;

            /// <summary>
            /// Gets the name of the node for the right side of the black box.
            /// </summary>
            public string Right { get; }

            /// <summary>
            /// Gets the name of the node for the bottom side of the black box.
            /// </summary>
            public string Bottom { get; }

            /// <inheritdoc/>
            public IPin this[string name]
            {
                get
                {
                    if (!_pinsByName.TryGetValue(name, out var pin))
                    {
                        pin = new LoosePin(name, name, _parent);
                        _pinsByName.Add(name, pin);
                        _pinsByIndex.Add(pin);
                    }
                    return pin;
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

                // This pin is not used, but avoids breaking things...
                var pin = new FixedPin("x", "x", _parent, new());
                _pinsByIndex.Add(pin);
            }

            /// <inheritdoc/>
            public IEnumerable<string> NamesOf(IPin pin)
            {
                yield return pin.Name;
            }

            /// <inheritdoc />
            public void Render(SvgDrawing drawing)
            {
                foreach (var pin in _pinsByIndex.OfType<LoosePin>().Where(p => !p.Orientation.X.IsZero() || !p.Orientation.Y.IsZero()))
                {
                    if (!string.IsNullOrWhiteSpace(pin.Name))
                        drawing.Text(pin.Name, pin.Location - pin.Orientation * 2, -pin.Orientation);
                }
            }

            /// <inheritdoc />
            public void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
            {
                var ckt = context.Circuit;
                var map = context.Nodes.Shorts;
                void Apply(string name, string start, IEnumerable<string> pinNodes, string end, double spacing)
                {
                    start = map[start];
                    end = map[end];
                    string lastNode = start;
                    int index = 0;
                    foreach (var node in pinNodes)
                    {
                        string n = map[node];
                        MinimumConstraint.AddMinimum(context.Circuit, $"{name}.{index++}", lastNode, n, spacing);
                        lastNode = n;
                    }
                    MinimumConstraint.AddMinimum(context.Circuit, $"{name}.{index++}", lastNode, end, spacing);
                }

                var pins = _pinsByIndex.OfType<LoosePin>();
                Apply($"{_parent.Name}.n", _parent.X, pins.Where(p => PointsUp(p)).Select(p => p.X), Right, MinimumHorizontalSpacing);
                Apply($"{_parent.Name}.s", _parent.X, pins.Where(p => PointsDown(p)).Select(p => p.X), Right, MinimumHorizontalSpacing);
                Apply($"{_parent.Name}.e", _parent.Y, pins.Where(p => PointsRight(p)).Select(p => p.Y), Bottom, MinimumVerticalSpacing);
                Apply($"{_parent.Name}.w", _parent.Y, pins.Where(p => PointsLeft(p)).Select(p => p.Y), Bottom, MinimumVerticalSpacing);
            }

            private static bool PointsUp(LoosePin pin) => Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y < 0;
            private static bool PointsDown(LoosePin pin) => Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y > 0;
            private static bool PointsLeft(LoosePin pin) => Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X < 0;
            private static bool PointsRight(LoosePin pin) => Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X > 0;

            /// <inheritdoc />
            public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
            {
                // We will group the X-coordinates and Y-coordinates of each side
                foreach (var pin in _pinsByIndex.OfType<LoosePin>())
                {
                    if (PointsUp(pin))
                        context.Shorts.Group(_parent.Y, pin.Y);
                    else if (PointsDown(pin))
                        context.Shorts.Group(Bottom, pin.Y);
                    else if (PointsLeft(pin))
                        context.Shorts.Group(_parent.X, pin.X);
                    else
                        context.Shorts.Group(Right, pin.X);

                    // Link the pin to the owner
                    context.Relative.Group(_parent.Y, pin.Y);
                    context.Relative.Group(_parent.X, pin.X);
                }
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

            /// <inheritdoc />
            public void Clear()
            {
                _pinsByIndex.Clear();
                _pinsByName.Clear();
            }
        }
    }
}
