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
        public void Prepare(GraphicalCircuit circuit, IDiagnosticHandler diagnostics)
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
                    diagnostics?.Post(_pinToWire.Component.Name, ErrorCodes.VirtualChainComponentNotFound, _pinToWire.Component.Fullname);
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
            }
        }

        public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            if (_info == null && _single != null && _single.Length > 1)
            {
                // Only single pin/wire is given, so let's align those with wildcards
                if ((_direction & Direction.X) != 0)
                {
                    string coord = null;
                    foreach (var c in _single)
                    {
                        if (coord == null)
                            coord = c.X;
                        else
                            context.Shorts.Group(coord, c.X);
                    }
                }
                if ((_direction & Direction.Y) != 0)
                {
                    string coord = null;
                    foreach (var c in _single)
                    {
                        if (coord == null)
                            coord = c.Y;
                        else
                            context.Shorts.Group(coord, c.Y);
                    }
                }
            }

            if (_info != null)
            {
                if ((_direction & Direction.X) != 0)
                {
                    if (_offset.X.IsZero() && !_extendLeft && !_extendRight)
                        context.Shorts.Group(_p2w.X, _w2p.X);
                }
                if ((_direction & Direction.Y) != 0)
                {
                    if (_offset.Y.IsZero() && !_extendDown && !_extendUp)
                        context.Shorts.Group(_p2w.Y, _w2p.Y);
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
