using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
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
            private readonly Dictionary<string, IPin> _pinsByName = [];
            private readonly List<IPin> _pinsByIndex = [];

            /// <summary>
            /// Gets the name of the node for the right side of the black box.
            /// </summary>
            public string Right { get; }

            /// <summary>
            /// Gets the name of the node for the bottom side of the black box.
            /// </summary>
            public string Bottom { get; }

            /// <summary>
            /// Gets or sets the corner radius (used for margins).
            /// </summary>
            public double CornerRadius { get; set; } = 0.0;

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
            public void Render(IGraphicsBuilder builder)
            {
                foreach (var pin in _pinsByIndex.OfType<LoosePin>().Where(p => !p.Orientation.X.IsZero() || !p.Orientation.Y.IsZero()))
                {
                    string name = TransformPinName(pin.Name);
                    if (name is not null)
                        builder.Text(name, pin.Location - pin.Orientation * 2, -pin.Orientation, _parent.TextSize);
                }
            }

            private string TransformPinName(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;
                if (name[0] == '_')
                    return null;
                return name;
            }

            /// <inheritdoc />
            public void Register(IRegisterContext context)
            {
                var ckt = context.Circuit;
                var map = context.Relationships.Offsets;
                double Apply(string name, string start, IEnumerable<string> pinNodes, string end, double spacing, double edgeSpacing)
                {
                    double width = 0.0;
                    var lastOffset = map[start];
                    var offsetEnd = map[end];
                    int index = 0;
                    foreach (var node in pinNodes)
                    {
                        var newOffset = map[node];
                        double currentSpace = index == 0 ? edgeSpacing : spacing;
                        MinimumConstraint.AddMinimum(context.Circuit, $"{name}.{index++}", lastOffset, newOffset, currentSpace, 100.0);
                        width += currentSpace;
                        lastOffset = newOffset;
                    }

                    // If there were no pins, we will just rely on the general minimum width
                    if (width > 0)
                        MinimumConstraint.AddMinimum(context.Circuit, $"{name}.{index++}", lastOffset, offsetEnd, edgeSpacing, 100.0);
                    width += edgeSpacing;
                    if (index == 0)
                        return 0.0;
                    return width;
                }

                var pins = _pinsByIndex.OfType<LoosePin>();
                double width = 0, height = 0;
                width = Math.Max(width, Apply($"{_parent.Name}.n", _parent.X, pins.Where(PointsUp).Select(p => p.X), Right, _parent.MinSpaceX, _parent.MinEdgeX + CornerRadius));
                width = Math.Max(width, Apply($"{_parent.Name}.s", _parent.X, pins.Where(PointsDown).Select(p => p.X), Right, _parent.MinSpaceX, _parent.MinEdgeX + CornerRadius));
                height = Math.Max(height, Apply($"{_parent.Name}.e", _parent.Y, pins.Where(PointsRight).Select(p => p.Y), Bottom, _parent.MinSpaceY, _parent.MinEdgeY + CornerRadius));
                height = Math.Max(height, Apply($"{_parent.Name}.w", _parent.Y, pins.Where(PointsLeft).Select(p => p.Y), Bottom, _parent.MinSpaceY, _parent.MinEdgeY + CornerRadius));

                if (width < _parent.MinWidth)
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.min.x", context.Relationships.Offsets[_parent.X], context.Relationships.Offsets[Right], _parent.MinWidth, 100.0);
                if (height < _parent.MinHeight)
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.min.y", context.Relationships.Offsets[_parent.Y], context.Relationships.Offsets[Bottom], _parent.MinHeight, 100.0);
            }

            private static bool PointsUp(LoosePin pin) => Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y < 0;
            private static bool PointsDown(LoosePin pin) => Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y > 0;
            private static bool PointsLeft(LoosePin pin) => Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X < 0;
            private static bool PointsRight(LoosePin pin) => Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X > 0;

            /// <inheritdoc />
            public PresenceResult Prepare(IPrepareContext context)
            {
                var pins = _pinsByIndex.OfType<LoosePin>();
                switch (context.Mode)
                {
                    case PreparationMode.Offsets:
                        foreach (var pin in pins)
                        {
                            if (PointsUp(pin))
                            {
                                if (!context.Offsets.Group(_parent.Y, pin.Y, 0.0))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongY, _parent.Y, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                            }
                            else if (PointsDown(pin))
                            {
                                if (!context.Offsets.Group(Bottom, pin.Y, 0.0))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongY, Bottom, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                            }
                            else if (PointsLeft(pin))
                            {
                                if (!context.Offsets.Group(_parent.X, pin.X, 0.0))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongX, _parent.X, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                            }
                            else if (PointsRight(pin))
                            {
                                if (!context.Offsets.Group(Right, pin.X, 0.0))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongX, _parent.Name, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                            }
                        }
                        break;

                    case PreparationMode.Groups:
                        // Everything will be linked to the parent X,Y coordinates
                        string x = context.Offsets[_parent.X].Representative;
                        string y = context.Offsets[_parent.Y].Representative;
                        foreach (var pin in pins)
                        {
                            string px = context.Offsets[pin.X].Representative;
                            string py = context.Offsets[pin.Y].Representative;
                            context.Group(x, px);
                            context.Group(y, py);
                        }
                        break;
                }
                return PresenceResult.Success;
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
