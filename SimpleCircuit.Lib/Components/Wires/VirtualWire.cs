using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SpiceSharp.Simulations;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A virtual wire.
    /// </summary>
    public class VirtualWire : ICircuitSolverPresence
    {
        private ILocatedPresence[] _single;
        private ILocatedPresence _p2w, _w2p;
        private readonly PinInfo _pinToWire, _wireToPin;
        private readonly WireInfo _info;
        private readonly Direction _direction;
        private Vector2 _offset;
        private bool _extendLeft, _extendUp, _extendRight, _extendDown;

        /// <summary>
        /// Enumeration of the directions along which the virtual wire acts
        /// </summary>
        [Flags]
        public enum Direction
        {
            None = 0x00,
            X = 0x01,
            Y = 0x02
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public int Order => 1;

        /// <summary>
        /// Creates a new <see cref="VirtualWire"/>.
        /// </summary>
        /// <param name="name">The name of the virtual wire.</param>
        /// <param name="pinToWire">The pin starting the virtual wire.</param>
        /// <param name="info">The wire information.</param>
        /// <param name="wireToPin">The pin ending the virtual wire.</param>
        public VirtualWire(string name, PinInfo pinToWire, WireInfo info, PinInfo wireToPin, Direction direction)
        {
            Name = name;
            _info = info;
            _pinToWire = pinToWire;
            _wireToPin = wireToPin;
            _direction = direction;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _single = null;
            _p2w = null;
            _w2p = null;
            _offset = new();
            _extendLeft = false;
            _extendRight = false;
            _extendUp = false;
            _extendDown = false;
        }

        /// <inheritdoc />
        public PresenceResult Prepare(GraphicalCircuit circuit, PresenceMode mode, IDiagnosticHandler diagnostics)
        {
            if (_info == null)
            {
                // We try to align many components here
                if (_pinToWire.Component.Fullname.Contains("*"))
                {
                    // Align all given pins of all named components that match the name with wildcard
                    string format = $"^{_pinToWire.Component.Fullname.Replace("*", "[\\w_]+")}$";
                    _single = circuit.OfType<ILocatedDrawable>().Where(p => Regex.IsMatch(p.Name, format)).ToArray();
                }
                else
                {
                    // Align all anonymous components that match the name
                    string lead = _pinToWire.Component.Fullname + DrawableFactoryDictionary.AnonymousSeparator;
                    _single = circuit.OfType<ILocatedDrawable>().Where(p => p.Name.StartsWith(lead)).ToArray();
                }

                if (_single == null || _single.Length == 0)
                {
                    diagnostics?.Post(_pinToWire.Component.Name, ErrorCodes.VirtualChainComponentNotFound, _pinToWire.Component.Fullname);
                    return PresenceResult.GiveUp;
                }
                return PresenceResult.Success;
            }
            else
            {
                // Find first pin
                if (circuit.TryGetValue(_pinToWire.Component.Fullname, out var presence) && presence is IDrawable p2wd)
                {
                    if (_pinToWire.Pin.Content.Length == 0 && p2wd is ILocatedPresence ld)
                        _p2w = ld;
                    else
                        _p2w = _pinToWire.Find(p2wd, diagnostics, -1);
                }

                // Find last pin
                if (circuit.TryGetValue(_wireToPin.Component.Fullname, out presence) && presence is IDrawable w2pd)
                {
                    if (_wireToPin.Pin.Content.Length == 0 && w2pd is ILocatedPresence ld)
                        _w2p = ld;
                    else
                        _w2p = _wireToPin.Find(w2pd, diagnostics, 0);
                }

                // Calculate the combined effect of the virtual wire
                var segments = _info.Segments;
                for (int i = 0; i < segments.Count; i++)
                {
                    var normal = segments[i].Orientation;
                    _offset += segments[i].Length * normal;
                    if (!segments[i].IsFixed)
                    {
                        if (normal.X < 0 && !normal.X.IsZero())
                            _extendLeft = true;
                        if (normal.X > 0 && !normal.X.IsZero())
                            _extendRight = true;
                        if (normal.Y < 0 && !normal.Y.IsZero())
                            _extendUp = true;
                        if (normal.Y > 0 && !normal.Y.IsZero())
                            _extendDown = true;
                    }
                }
                return PresenceResult.Success;
            }
        }

        public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            if (_info == null && _single != null && _single.Length > 1)
            {
                // This is an alignment of many items
                switch (context.Mode)
                {
                    case NodeRelationMode.Shorts:
                        if ((_direction & Direction.X) != 0)
                        {
                            string coord = null;
                            foreach (var p in _single)
                            {
                                if (coord == null)
                                    coord = p.X;
                                else
                                    context.Shorts.Group(coord, p.X);
                            }
                        }
                        if ((_direction & Direction.Y) != 0)
                        {
                            string coord = null;
                            foreach (var p in _single)
                            {
                                if (coord == null)
                                    coord = p.Y;
                                else
                                    context.Shorts.Group(coord, p.Y);
                            }
                        }
                        break;

                    default:
                        break;
                }
            }

            if (_info != null)
            {
                // This is a real virtual wire, with wire segments
                // We already combined all info in the Prepare method
                if (_p2w == null || _w2p == null)
                    return;

                switch (context.Mode)
                {
                    case NodeRelationMode.Shorts:
                        if ((_direction & Direction.X) != 0)
                        {
                            if (!_extendLeft && !_extendRight)
                            {
                                if (_offset.X.IsZero())
                                    context.Shorts.Group(_p2w.X, _w2p.X);
                            }
                        }
                        if ((_direction & Direction.Y) != 0)
                        {
                            if (!_extendLeft && !_extendRight)
                            {
                                if (_offset.Y.IsZero())
                                    context.Shorts.Group(_p2w.Y, _w2p.Y);
                            }
                        }
                        break;

                    case NodeRelationMode.Links:
                        if ((_direction & Direction.X) != 0)
                        {
                            if (_extendLeft && _extendRight)
                            {
                                // Nothing can be inferred here...
                            }
                            else if (_extendRight)
                                context.Extremes.Order(context.Shorts[_p2w.X], context.Shorts[_w2p.X]);
                            else if (_extendLeft)
                                context.Extremes.Order(context.Shorts[_w2p.X], context.Shorts[_p2w.X]);
                        }
                        if ((_direction & Direction.Y) != 0)
                        {
                            if (_extendUp && _extendDown)
                            {
                                // Nothing can be inferred here...
                            }
                            else if (_extendDown)
                                context.Extremes.Order(context.Shorts[_p2w.Y], context.Shorts[_w2p.Y]);
                            else if (_extendUp)
                                context.Extremes.Order(context.Shorts[_w2p.Y], context.Shorts[_p2w.Y]);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            // All single components should have been aligned already, only consider if wire info is available
            if (_info != null && _p2w != null && _w2p != null)
            {
                if ((_direction & Direction.X) != 0)
                {
                    // If we can extend wherever, don't deal with this
                    string name = $"{Name}.x";
                    string x1 = context.Nodes.Shorts[_p2w.X];
                    string x2 = context.Nodes.Shorts[_w2p.X];
                    if (!_extendLeft && !_extendRight)
                        OffsetConstraint.AddOffset(context.Circuit, name, x1, x2, _offset.X);
                    else if (!_extendLeft)
                        MinimumConstraint.AddMinimum(context.Circuit, name, x1, x2, _offset.X);
                    else if (!_extendRight)
                        MinimumConstraint.AddMinimum(context.Circuit, name, x2, x1, -_offset.X);
                }
                if ((_direction & Direction.Y) != 0)
                {
                    string name = $"{Name}.y";
                    string y1 = context.Nodes.Shorts[_p2w.Y];
                    string y2 = context.Nodes.Shorts[_w2p.Y];
                    if (!_extendUp && !_extendDown)
                        OffsetConstraint.AddOffset(context.Circuit, name, y1, y2, _offset.Y);
                    else if (!_extendUp)
                        MinimumConstraint.AddMinimum(context.Circuit, name, y1, y2, _offset.Y);
                    else if (!_extendDown)
                        MinimumConstraint.AddMinimum(context.Circuit, name, y2, y1, -_offset.Y);
                }
            }
        }

        /// <inheritdoc />
        public void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
        }
    }
}
