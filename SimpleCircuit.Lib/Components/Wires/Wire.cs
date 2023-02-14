using SimpleCircuit.Components.Digital;
using SimpleCircuit.Components.General;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire that can have a variable length.
    /// </summary>
    public class Wire : Drawable
    {
        private readonly struct WirePoint
        {
            public bool IsJumpOver { get; }
            public Vector2 Location { get; }
            public WirePoint(Vector2 location, bool isJumpOver)
            {
                Location = location;
                IsJumpOver = isJumpOver;
            }
            public override string ToString() => Location.ToString();
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

            if (_info.Segments.Count == 1 && _info.Segments[0].Orientation.X.IsZero() && _info.Segments[0].Orientation.Y.IsZero() && !_info.Segments[0].IsUnconstrained)
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
                // In case we have multiple wires, we might have some boundary conditions that we need to be taking care of
                for (int i = 0; i < _info.Segments.Count; i++)
                {
                    var segment = _info.Segments[i];
                    if (segment.IsUnconstrained)
                        continue;
                    else if (segment.Orientation.X.IsZero() && segment.Orientation.Y.IsZero())
                    {
                        if (i == 0)
                        {
                            // Check with first pin, and only with first pin...
                            if (p1 == null)
                            {
                                GenerateError(diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Component.Fullname);
                                return PresenceResult.GiveUp;
                            }
                        }
                        else if (i == _info.Segments.Count - 1)
                        {
                            if (p2 == null)
                            {
                                GenerateError(diagnostics, _wireToPin, ErrorCodes.AmbiguousOrientation, _w2p.Name, _wireToPin.Component.Fullname);
                                return PresenceResult.GiveUp;
                            }
                        }
                        else
                        {
                            diagnostics?.Post(segment.Source, ErrorCodes.UndefinedWireSegment);
                            return PresenceResult.GiveUp;
                        }
                    }
                }
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public override bool DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            string x, y;
            switch (context.Mode)
            {
                case NodeRelationMode.Offsets:

                    // Short wire ends to the correct pins
                    if (_p2w != null)
                    {
                        if (!context.Offsets.Group(_p2w.X, StartX, 0.0))
                        {
                            diagnostics?.Post(ErrorCodes.CannotAlignAlongX, _p2w.X, StartX);
                            return false;
                        }
                        if (!context.Offsets.Group(_p2w.Y, StartY, 0.0))
                        {
                            diagnostics?.Post(ErrorCodes.CannotAlignAlongY, _p2w.Y, StartY);
                            return false;
                        }
                    }
                    if (_w2p != null)
                    {
                        if (!context.Offsets.Group(_w2p.X, EndX, 0.0))
                        {
                            diagnostics?.Post(ErrorCodes.CannotAlignAlongX, _w2p.X, EndX);
                            return false;
                        }
                        if (!context.Offsets.Group(_w2p.Y, EndY, 0.0))
                        {
                            diagnostics?.Post(ErrorCodes.CannotAlignAlongY, _w2p.Y, EndY);
                            return false;
                        }
                    }

                    // Deal with horizontal and vertical segments
                    x = StartX;
                    y = StartY;
                    for (int i = 0; i < _info.Segments.Count; i++)
                    {
                        string tx = GetXName(i);
                        string ty = GetYName(i);
                        var segment = _info.Segments[i];

                        // Ignore unconstrained wires
                        if (!segment.IsUnconstrained)
                        {
                            var orientation = GetOrientation(i);
                            if (segment.IsFixed)
                            {
                                double l = segment.Length;

                                if (!context.Offsets.Group(x, tx, orientation.X * l))
                                {
                                    diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(orientation.X * l), segment.Source.Content);
                                    return false;
                                }
                                if (!context.Offsets.Group(y, ty, orientation.Y * l))
                                {
                                    diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(orientation.Y * l), segment.Source.Content);
                                    return false;
                                }
                            }
                            else
                            {
                                if (orientation.X.IsZero())
                                {
                                    if (!context.Offsets.Group(x, tx, 0.0))
                                    {
                                        diagnostics?.Post(segment.Source, ErrorCodes.CannotAlignAlongX, x, tx);
                                        return false;
                                    }
                                }
                                if (orientation.Y.IsZero())
                                {
                                    if (!context.Offsets.Group(y, ty, 0.0))
                                    {
                                        diagnostics?.Post(segment.Source, ErrorCodes.CannotAlignAlongY, y, ty);
                                        return false;
                                    }
                                }
                            }
                        }
                        x = tx;
                        y = ty;
                    }
                    break;

                case NodeRelationMode.Links:

                    // Go through all segments and determine the order of coordinates
                    var offsetX = context.Offsets[StartX];
                    var offsetY = context.Offsets[StartY];
                    for (int i = 0; i < _info.Segments.Count; i++)
                    {
                        var toOffsetX = context.Offsets[GetXName(i)];
                        var toOffsetY = context.Offsets[GetYName(i)];
                        var segment = _info.Segments[i];
                        if (!segment.IsUnconstrained && !segment.IsFixed)
                        {
                            var orientation = GetOrientation(i);

                            // If the wire is length is fixed, then we just need to compare their relative offset
                            if (!orientation.X.IsZero())
                            {
                                if (!MinimumConstraint.MinimumDirectionalLink(context, offsetX, toOffsetX, orientation.X * segment.Length))
                                    diagnostics?.Post(segment.Source, ErrorCodes.CouldNotSatisfyMinimumOfForInX, Math.Abs(orientation.X * segment.Length), segment.Source.Content);
                            }
                            if (!orientation.Y.IsZero())
                            {
                                if (!MinimumConstraint.MinimumDirectionalLink(context, offsetY, toOffsetY, orientation.Y * segment.Length))
                                    diagnostics?.Post(segment.Source, ErrorCodes.CouldNotSatisfyMinimumOfForInY, Math.Abs(orientation.Y * segment.Length), segment.Source.Content);
                            }
                        }
                        offsetX = toOffsetX;
                        offsetY = toOffsetY;
                    }
                    break;
            }
            return true;
        }

        private Vector2 GetOrientation(int index)
        {
            var orientation = _info.Segments[index].Orientation;
            if (orientation.X.IsZero() && orientation.Y.IsZero())
            {
                // Take the pin orientation instead
                if (index == 0 && _p2w is IOrientedPin p1)
                    orientation = p1.Orientation;
                else if (index == _info.Segments.Count - 1 && _w2p is IOrientedPin p2)
                    orientation = -p2.Orientation;
                else
                    orientation = new(1, 0);
            }
            return orientation;
        }

        /// <inheritdoc />
        public override void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            // Extract the first node
            var last = context.Nodes.GetValue(state, StartX, StartY);
            int count = context.WireSegments.Count;
            _vectors.Add(new(last, false));
            for (int i = 0; i < _info.Segments.Count; i++)
            {
                var next = context.Nodes.GetValue(state, GetXName(i), GetYName(i));

                // Add jump-over points if specified
                if (_info.JumpOverWires)
                    AddJumpOverWires(last, next, context.WireSegments.Take(count));

                _vectors.Add(new(next, false));
                context.WireSegments.Add(new(last, next));
                if (_vectors.Count > 2)
                    count++;
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
            var map = context.Nodes.Offsets;
            var fromX = map[StartX];
            var fromY = map[StartY];
            for (int i = 0; i < _info.Segments.Count; i++)
            {
                string x = GetXName(i);
                string y = GetYName(i);
                var toX = map[x];
                var toY = map[y];
                var segment = _info.Segments[i];
                if (!segment.IsUnconstrained && !segment.IsFixed)
                {
                    var orientation = GetOrientation(i);

                    // Wire is of minimum length
                    if (orientation.X.IsZero())
                        MinimumConstraint.AddDirectionalMinimum(context.Circuit, y, fromY, toY, orientation.Y * segment.Length);
                    if (orientation.Y.IsZero())
                        MinimumConstraint.AddDirectionalMinimum(context.Circuit, x, fromX, toX, orientation.X * segment.Length);
                    if (!orientation.X.IsZero() && !orientation.Y.IsZero())
                    {
                        // Odd-angle wire segment
                        string ox, oy;
                        double dx = fromX.Offset - toX.Offset;
                        double dy = fromY.Offset - toY.Offset;
                        if (dx.IsZero())
                            ox = fromX.Representative;
                        else
                        {
                            ox = $"{x}.ox";
                            context.Circuit.Add(new VoltageSource($"V{x}.ox", ox, fromX.Representative, fromX.Offset - toX.Offset));
                        }
                        if (dy.IsZero())
                            oy = fromY.Representative;
                        else
                        {
                            oy = $"{y}.oy";
                            context.Circuit.Add(new VoltageSource($"V{y}.oy", oy, fromY.Representative, fromY.Offset - toY.Offset));
                        }
                        string tx = toX.Representative;
                        string ty = toY.Representative;

                        // General case, in any direction
                        // Link the X and Y coordinates such that the slope remains correct
                        string inside = $"{x}.xc";
                        MinimumConstraint.AddRectifyingElement(context.Circuit, $"D{inside}", inside, tx);
                        context.Circuit.Add(new VoltageControlledVoltageSource($"E{inside}", inside, ox, ty, oy, orientation.X / orientation.Y));

                        inside = $"{y}.yc";
                        MinimumConstraint.AddRectifyingElement(context.Circuit, $"D{inside}", inside, ty);
                        context.Circuit.Add(new VoltageControlledVoltageSource($"E{inside}", inside, oy, tx, ox, orientation.Y / orientation.X));

                        // Make sure the X and Y length cannot go below their theoretical minimum
                        MinimumConstraint.AddDirectionalMinimum(context.Circuit, $"{x}.mx", ox, tx, orientation.X * segment.Length);
                        MinimumConstraint.AddDirectionalMinimum(context.Circuit, $"{y}.my", oy, ty, orientation.Y * segment.Length);
                    }
                }
                fromX = toX;
                fromY = toY;
            }
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            PathOptions markerOptions = new("marker");
            List<(Vector2 Location, Vector2 Normal, MarkerTypes Type)> markers = new();
            if (_info.IsVisible && _vectors.Count > 0)
            {
                WirePoint point = _vectors[0];
                drawing.Path(builder =>
                {
                    builder.MoveTo(point.Location);
                    int segment = 0;
                    MarkerTypes startMarker = _info.Segments[0].StartMarker;
                    for (int i = 1; i < _vectors.Count; i++)
                    {
                        point = _vectors[i];

                        // Draw a small half circle for crossing over this point
                        if (_vectors[i].IsJumpOver)
                        {
                            GetNewAxes(_vectors[i - 1].Location, _vectors[i].Location, out var nx, out var ny);
                            Vector2 o = _vectors[i].Location;
                            Vector2 s = o - nx * _jumpOverRadius;
                            Vector2 e = o + _jumpOverRadius * nx;
                            Vector2 m = o + ny * _jumpOverRadius;

                            builder.LineTo(s);

                            // Deal with the start marker
                            if (startMarker != MarkerTypes.None)
                                markers.Add((builder.Start, builder.StartNormal, startMarker));
                            startMarker = MarkerTypes.None;

                            nx *= 0.55 * _jumpOverRadius;
                            ny *= 0.55 * _jumpOverRadius;
                            builder.CurveTo(s + ny, m - nx, m);
                            builder.SmoothTo(e + ny, e);
                        }
                        else
                        {
                            builder.LineTo(_vectors[i].Location);

                            // Deal with the start marker
                            if (startMarker != MarkerTypes.None)
                                markers.Add((builder.Start, builder.StartNormal, startMarker));
                            startMarker = MarkerTypes.None;

                            // Deal with the end marker
                            var endMarker = _info.Segments[segment].EndMarker;
                            if (endMarker != MarkerTypes.None)
                                markers.Add((builder.End, builder.EndNormal, endMarker));
                            segment++;

                            // Prepare for the next start marker
                            if (segment < _info.Segments.Count)
                                startMarker = _info.Segments[segment].StartMarker;
                        }
                    }
                }, _info.Options);

                // Draw the markers (if any)
                if (markers.Count > 0)
                {
                    foreach (var marker in markers)
                        drawing.Marker(marker.Type, marker.Location, marker.Normal, marker.Type.PathOptions());
                }
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
