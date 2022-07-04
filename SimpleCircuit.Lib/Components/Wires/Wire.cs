using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire that can have a variable length.
    /// </summary>
    public class Wire : Drawable
    {
        private struct WirePoint
        {
            public bool IsJumpOver { get; }
            public Vector2 Location { get; }
            public WirePoint(Vector2 location, bool isJumpOver)
            {
                Location = location;
                IsJumpOver = isJumpOver;
            }
        }

        private IPin _w2p, _p2w;
        private readonly PinInfo _wireToPin, _pinToWire;
        private readonly WireInfo _info;
        private readonly List<WirePoint> _vectors = new();
        private const double _jumpOverRadius = 1.5;

        /// <inheritdoc />
        public override int Order => -1;

        /// <inheritdoc />
        public override string Type => "wire";

        /// <summary>
        /// Gets the X-coordinate name of the first point of the wire.
        /// </summary>
        public string StartX => GetXName(-1);

        /// <summary>
        /// Gets the Y-coordinate name of the first point of the wire.
        /// </summary>
        public string StartY => GetYName(-1);

        /// <summary>
        /// Gets the X-coordinate name of the last point of the wire.
        /// </summary>
        public string EndX => GetXName(_info.Segments.Count - 1);

        /// <summary>
        /// Gets the Y-coordinate name of the last point of the wire.
        /// </summary>
        public string EndY => GetYName(_info.Segments.Count - 1);

        /// <summary>
        /// Creates a new wire.
        /// </summary>
        /// <param name="name">The name of the wire.</param>
        /// <param name="pinToWire">The pin that will start the wire.</param>
        /// <param name="info">The wire information.</param>
        /// <param name="wireToPin">The pin that will end the wire.</param>
        public Wire(string name, PinInfo pinToWire, WireInfo info, PinInfo wireToPin)
            : base(name)
        {
            _pinToWire = pinToWire;
            _info = info ?? throw new ArgumentNullException(nameof(info));
            _wireToPin = wireToPin;
        }

        /// <inheritdoc />
        public override void Reset()
        {
            _vectors.Clear();
            _p2w = null;
            _w2p = null;
        }

        /// <inheritdoc />
        public override PresenceResult Prepare(GraphicalCircuit circuit, PresenceMode mode, IDiagnosticHandler diagnostics)
        {
            // Find the pins
            _p2w = _pinToWire.Find(diagnostics, -1);
            _w2p = _wireToPin.Find(diagnostics, 0);

            // Make sure these pins know they are being connected to
            if (_p2w != null)
                _p2w.Connections++;
            if (_w2p != null)
                _w2p.Connections++;

            var p1 = _p2w as IOrientedPin;
            var p2 = _w2p as IOrientedPin;

            if (_info.Segments.Count == 1 && _info.Segments[0].Orientation.X.IsZero() && _info.Segments[0].Orientation.Y.IsZero())
            {
                // We have a wire that connects two pins but does not have a defined orientation yet
                // This piece of code will pass this orientation to other nodes if they can be resolved
                if (p1 == null && p2 == null)
                {
                    GenerateError(diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Component.Fullname);
                    return PresenceResult.GiveUp;
                }
                else if (p1 != null && p2 != null)
                {
                    // We might need to point it towards each other
                    bool p1f = p1.HasFixedOrientation, p2f = p2.HasFixedOrientation;
                    if (p1f && p2f)
                        // Everything has been fixed already!
                        return PresenceResult.Success;
                    else if (p1f)
                        // The first pin is fixed, so let's use its orientation to set the second one
                        p2.ResolveOrientation(-p1.Orientation, diagnostics);
                    else if (p2f)
                        // The second pin is fixed, so let's use its orientation to set the first one
                        p1.ResolveOrientation(-p2.Orientation, diagnostics);
                    else
                    {
                        // Both aren't fixed, so we might just not have enough information...
                        if (mode == PresenceMode.GiveUp)
                        {
                            GenerateError(diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Component.Fullname);
                            return PresenceResult.GiveUp;
                        }
                        return PresenceResult.Incomplete;
                    }
                }
                else if (p1 != null)
                {
                    // We can only try to derive from the first pin!
                    if (p1.HasFixedOrientation)
                        return PresenceResult.Success; // We can derive the orientation
                    if (mode == PresenceMode.GiveUp)
                    {
                        GenerateError(diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Component.Fullname);
                        return PresenceResult.GiveUp;
                    }
                    else
                        return PresenceResult.Incomplete;
                }
                else
                {
                    // We can only try to derive from the second pin!
                    if (p2.HasFixedOrientation)
                        return PresenceResult.Success;
                    if (mode == PresenceMode.GiveUp)
                    {
                        GenerateError(diagnostics, _wireToPin, ErrorCodes.AmbiguousOrientation, _w2p.Name, _wireToPin.Component.Fullname);
                        return PresenceResult.GiveUp;
                    }
                    else
                        return PresenceResult.Incomplete;
                }
            }
            else
            {
                // We have multiple wires, but we want to make sure that the wire doesn't contain unresolved segment
                // orientations in the middle...
                for (int i = 0; i < _info.Segments.Count; i++)
                {
                    var segment = _info.Segments[i];
                    if (segment.Orientation.X.IsZero() && segment.Orientation.Y.IsZero())
                    {
                        if (i != 0 && i != _info.Segments.Count - 1)
                        {
                            diagnostics?.Post(_info.Segments[i].Source, ErrorCodes.UndefinedWireSegment);
                            return PresenceResult.GiveUp;
                        }
                    }
                }
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            // Align the pins
            if (_p2w != null)
            {
                context.Shorts.Group(_p2w.X, StartX);
                context.Shorts.Group(_p2w.Y, StartY);
                context.Relative.Group(_p2w.X, StartX);
                context.Relative.Group(_p2w.Y, StartY);
            }
            if (_w2p != null)
            {
                context.Shorts.Group(_w2p.X, EndX);
                context.Shorts.Group(_w2p.Y, EndY);
                context.Relative.Group(_w2p.X, EndX);
                context.Relative.Group(_w2p.Y, EndY);
            }

            // The segments themselves
            string x = StartX, y = StartY;
            for (int i = 0; i < _info.Segments.Count; i++)
            {
                string tx = GetXName(i), ty = GetYName(i);
                Vector2 orientation = _info.Segments[i].Orientation;
                if (orientation.X.IsZero() && orientation.Y.IsZero())
                {
                    // Take the pin orientation instead
                    if (i == 0 && _p2w is IOrientedPin p1)
                        orientation = p1.Orientation;
                    else if (i == _info.Segments.Count - 1 && _w2p is IOrientedPin p2)
                        orientation = p2.Orientation;
                    else
                        orientation = new(1, 0);
                }
                if (orientation.X.IsZero())
                    context.Shorts.Group(x, tx);
                if (orientation.Y.IsZero())
                    context.Shorts.Group(y, ty);
                context.Relative.Group(x, tx);
                context.Relative.Group(y, ty);
                x = tx;
                y = ty;
            }
        }

        /// <inheritdoc />
        public override void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            double x = 0.0, y = 0.0;
            if (state.TryGetValue(context.Nodes.Shorts[StartX], out var solX))
                x = solX.Value;
            if (state.TryGetValue(context.Nodes.Shorts[StartY], out var solY))
                y = solY.Value;
            Vector2 last = new(x, y);
            _vectors.Add(new(last, false));

            int count = context.WireSegments.Count;
            for (int i = 0; i < _info.Segments.Count; i++)
            {
                x = 0.0;
                y = 0.0;
                if (state.TryGetValue(context.Nodes.Shorts[GetXName(i)], out solX))
                    x = solX.Value;
                if (state.TryGetValue(context.Nodes.Shorts[GetYName(i)], out solY))
                    y = solY.Value;
                Vector2 next = new(x, y);

                // Add jump-over points if specified
                if (_info.JumpOverWires)
                    AddJumpOverWires(last, next, context.WireSegments.Take(count));

                _vectors.Add(new(next, false));
                context.WireSegments.Add(new(last, next));
                last = next;
            }
        }

        private void AddJumpOverWires(Vector2 last, Vector2 next, IEnumerable<WireSegment> segments)
        {
            // Calculate overlapping vectors
            Vector2 d = last - next;
            SortedDictionary<double, Vector2> pts = new();
            foreach (var segment in segments)
            {
                Vector2 sd = segment.Start - segment.End;
                double denom = d.X * sd.Y - d.Y * sd.X;
                if (denom.IsZero())
                    continue;

                double tn = (last.X - segment.Start.X) * sd.Y - (last.Y - segment.Start.Y) * sd.X;
                double un = (last.X - segment.Start.X) * d.Y - (last.Y - segment.Start.Y) * d.X;
                if (denom < 0)
                {
                    denom = -denom;
                    tn = -tn;
                    un = -un;
                }
                double tol = 1e-3 * denom;
                if (tn <= tol || un <= tol)
                    continue;
                tol = denom - tol;
                if (tn >= tol || un >= tol)
                    continue;
                Vector2 intersection = last - tn / denom * d;
                sd = intersection - last;
                pts.Add(sd.X * sd.X + sd.Y * sd.Y, intersection);
            }
            if (pts.Count > 0)
            {
                double minDist = _jumpOverRadius * _jumpOverRadius;
                double maxDist = (d.X * d.X + d.Y * d.Y) - minDist;
                foreach (var pair in pts)
                {
                    if (pair.Key >= minDist && pair.Key <= maxDist)
                        _vectors.Add(new(pair.Value, true));
                }
            }
        }

        /// <inheritdoc />
        public override void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            string x = StartX, y = StartY;
            for (int i = 0; i < _info.Segments.Count; i++)
            {
                string tx = GetXName(i), ty = GetYName(i);
                var defaultOrientation = new Vector2(1, 0);
                if (i == 0 && _p2w is IOrientedPin op)
                    defaultOrientation = op.Orientation;
                else if (i == _info.Segments.Count - 1 && _w2p is IOrientedPin op2)
                    defaultOrientation = -op2.Orientation;
                RegisterWire(context, x, y, tx, ty, _info.Segments[i], defaultOrientation);
                x = tx;
                y = ty;
            }
        }

        private void RegisterWire(CircuitSolverContext context, string x, string y, string tx, string ty, WireSegmentInfo segment, Vector2 defaultOrientation)
        {
            var map = context.Nodes.Shorts;
            string ox = map[x];
            string oy = map[y];
            tx = map[tx];
            ty = map[ty];

            // Get the orientation
            var direction = segment.Orientation;
            if (direction.X.IsZero() && direction.Y.IsZero())
                direction = defaultOrientation;
            direction = direction.Order(ref ox, ref tx, ref oy, ref ty);

            // Deal with the segment itself
            if (segment.IsFixed)
            {
                // Fixed length segment
                if (ox != tx)
                    OffsetConstraint.AddOffset(context.Circuit, x, ox, tx, direction.X * segment.Length);
                if (oy != ty)
                    OffsetConstraint.AddOffset(context.Circuit, y, oy, ty, direction.Y * segment.Length);
            }
            else
            {
                // Minimum length segment
                if (ox == tx)
                    MinimumConstraint.AddMinimum(context.Circuit, y, oy, ty, segment.Length);
                else if (oy == ty)
                    MinimumConstraint.AddMinimum(context.Circuit, x, ox, tx, segment.Length);
                else
                {
                    // General case, in any direction
                    // Link the X and Y coordinates such that the slope remains correct
                    string i = $"{x}.xc";
                    MinimumConstraint.AddRectifyingElement(context.Circuit, $"D{i}", i, tx);
                    context.Circuit.Add(new VoltageControlledVoltageSource($"E{i}", i, ox, ty, oy, direction.X / direction.Y));

                    i = $"{y}.yc";
                    MinimumConstraint.AddRectifyingElement(context.Circuit, $"D{i}", i, ty);
                    context.Circuit.Add(new VoltageControlledVoltageSource($"E{i}", i, oy, tx, ox, direction.Y / direction.X));

                    // Make sure the X and Y length cannot go below their theoretical minimum
                    MinimumConstraint.AddMinimum(context.Circuit, $"{x}.mx", ox, tx, direction.X * segment.Length);
                    MinimumConstraint.AddMinimum(context.Circuit, $"{y}.my", oy, ty, direction.Y * segment.Length);
                }
            }
        }

        protected override void Draw(SvgDrawing drawing)
        {
            if (_info.IsVisible && _vectors.Count > 0)
            {
                drawing.Path(builder =>
                {
                    // Start the first point and build the path
                    builder.MoveTo(_vectors[0].Location);
                    for (int i = 1; i < _vectors.Count; i++)
                    {
                        // Draw a small half circle for crossing over this point
                        if (_vectors[i].IsJumpOver)
                        {
                            GetNewAxes(_vectors[i].Location, _vectors[i - 1].Location, out var nx, out var ny);
                            Vector2 o = _vectors[i].Location;
                            Vector2 s = o - nx * _jumpOverRadius;
                            Vector2 e = o + _jumpOverRadius * nx;
                            Vector2 m = o + ny * _jumpOverRadius;

                            builder.LineTo(s);
                            nx *= 0.55 * _jumpOverRadius;
                            ny *= 0.55 * _jumpOverRadius;
                            builder.CurveTo(s + ny, m - nx, m);
                            builder.SmoothTo(e + ny, e);
                        }
                        else
                            builder.LineTo(_vectors[i].Location);
                    }
                }, _info.Options);
            }
        }

        private void GetNewAxes(Vector2 a, Vector2 b, out Vector2 nx, out Vector2 ny)
        {
            // Get the normal of the wire segment
            nx = b - a;
            if (!nx.X.IsZero() || !nx.Y.IsZero())
                nx /= nx.Length;
            else
            {
                ny = new();
                return;
            }

            // Perpendicular direction
            ny = new(nx.Y, -nx.X);
            if (Math.Abs(ny.Y) > Math.Abs(ny.X))
            {
                if (ny.Y > 0)
                    ny = -ny; // Choose upward direction
            }
            else
            {
                if (ny.X > 0)
                    ny = -ny; // Choose leftward direction
            }
        }
        private string GetXName(int index) => $"{Name}.{index + 1}.x";
        private string GetYName(int index) => $"{Name}.{index + 1}.y";

        private void GenerateError(IDiagnosticHandler diagnostics, PinInfo pin, ErrorCodes code, params object[] arguments)
        {
            if (pin.Pin.Content.Length > 0)
                diagnostics?.Post(pin.Pin, code, arguments);
            else
                diagnostics?.Post(pin.Component.Name, code, arguments);
        }
    }
}
