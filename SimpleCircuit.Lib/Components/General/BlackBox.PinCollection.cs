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
            public double MinSpaceX { get; set; } = 20.0;

            /// <summary>
            /// Gets or sets the minimum horizontal spacing at the edges.
            /// </summary>
            public double MinEdgeX { get; set; } = 10.0;

            /// <summary>
            /// Gets or sets the minimum vertical spacing.
            /// </summary>
            public double MinSpaceY { get; set; } = 10.0;

            /// <summary>
            /// Gets or sets the minimum vertical spacing at the edges.
            /// </summary>
            public double MinEdgeY { get; set; } = 5.0;

            /// <summary>
            /// Gets or sets the minimum width.
            /// </summary>
            public double MinWidth { get; set; } = 30;

            /// <summary>
            /// Gets or sets the minimum height.
            /// </summary>
            public double MinHeight { get; set; } = 10;

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
            public void Register(CircuitSolverContext context)
            {
                var ckt = context.Circuit;
                var map = context.Nodes.Shorts;
                double Apply(string name, string start, IEnumerable<string> pinNodes, string end, double spacing, double edgeSpacing)
                {
                    double width = 0.0;
                    start = map[start];
                    end = map[end];
                    string lastNode = start;
                    int index = 0;
                    foreach (var node in pinNodes)
                    {
                        string n = map[node];
                        double currentSpace = index == 0 ? edgeSpacing : spacing;
                        MinimumConstraint.AddMinimum(context.Circuit, $"{name}.{index++}", lastNode, n, currentSpace, 100.0);
                        width += currentSpace;
                        lastNode = n;
                    }
                    MinimumConstraint.AddMinimum(context.Circuit, $"{name}.{index++}", lastNode, end, edgeSpacing, 100.0);
                    width += edgeSpacing;
                    return width;
                }

                var pins = _pinsByIndex.OfType<LoosePin>();
                double width = 0, height = 0;
                width = Math.Max(width, Apply($"{_parent.Name}.n", _parent.X, pins.Where(PointsUp).Select(p => p.X), Right, MinSpaceX, MinEdgeX));
                width = Math.Max(width, Apply($"{_parent.Name}.s", _parent.X, pins.Where(PointsDown).Select(p => p.X), Right, MinSpaceX, MinEdgeX));
                height = Math.Max(height, Apply($"{_parent.Name}.e", _parent.Y, pins.Where(PointsRight).Select(p => p.Y), Bottom, MinSpaceY, MinEdgeY));
                height = Math.Max(height, Apply($"{_parent.Name}.w", _parent.Y, pins.Where(PointsLeft).Select(p => p.Y), Bottom, MinSpaceY, MinEdgeY));

                if (width < MinWidth)
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.min.x", context.Nodes.Shorts[_parent.X], context.Nodes.Shorts[Right], MinWidth);
                if (height < MinHeight)
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.min.y", context.Nodes.Shorts[_parent.Y], context.Nodes.Shorts[Bottom], MinHeight);
            }

            private static bool PointsUp(LoosePin pin) => Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y < 0;
            private static bool PointsDown(LoosePin pin) => Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y > 0;
            private static bool PointsLeft(LoosePin pin) => Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X < 0;
            private static bool PointsRight(LoosePin pin) => Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X > 0;

            /// <inheritdoc />
            public void DiscoverNodeRelationships(NodeContext context)
            {
                var pins = _pinsByIndex.OfType<LoosePin>();
                switch (context.Mode)
                {
                    case NodeRelationMode.Shorts:
                        foreach (var pin in pins)
                        {
                            if (PointsUp(pin))
                                context.Shorts.Group(_parent.Y, pin.Y);
                            else if (PointsDown(pin))
                                context.Shorts.Group(Bottom, pin.Y);
                            else if (PointsLeft(pin))
                                context.Shorts.Group(_parent.X, pin.X);
                            else if (PointsRight(pin))
                                context.Shorts.Group(Right, pin.X);
                        }
                        break;

                    case NodeRelationMode.Links:
                        OrderCoordinates(pins.Where(PointsUp).Select(p => p.X), _parent.X, Right, context);
                        OrderCoordinates(pins.Where(PointsDown).Select(p => p.X), _parent.X, Right, context);
                        OrderCoordinates(pins.Where(PointsLeft).Select(p => p.Y), _parent.Y, Bottom, context);
                        OrderCoordinates(pins.Where(PointsRight).Select(p => p.Y), _parent.Y, Bottom, context);
                        break;

                    default:
                        return;
                }
            }

            private void OrderCoordinates(IEnumerable<string> coordinates, string first, string final, NodeContext context)
            {
                string lastCoordinate = context.Shorts[first];
                foreach (var coordinate in coordinates)
                {
                    var c = context.Shorts[coordinate];
                    context.Extremes.Order(lastCoordinate, c);
                    lastCoordinate = c;
                }
                context.Extremes.Order(lastCoordinate, context.Shorts[final]);
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
