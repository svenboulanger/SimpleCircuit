using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Components.Markers;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire that can have a variable length.
    /// </summary>
    public class Wire : Drawable
    {
        private IPin _w2p, _p2w;
        private readonly PinReference _wireToPin, _pinToWire;
        private readonly List<WireSegmentInfo> _segments;
        private readonly List<WirePoint> _localPoints = [];
        private readonly List<Vector2> _points = [];
        private const double _jumpOverRadius = 1.5;

        /// <summary>
        /// The variant used for invisible wires.
        /// </summary>
        public const string Hidden = "hidden";

        /// <summary>
        /// The variant used for a wire jumping over previous wires.
        /// </summary>
        public const string JumpOver = "jump";

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
        public string EndX => GetXName(_segments.Count - 1);

        /// <summary>
        /// Gets the Y-coordinate name of the last point of the wire.
        /// </summary>
        public string EndY => GetYName(_segments.Count - 1);

        /// <summary>
        /// Gets or sets the radius of corners.
        /// </summary>
        [Description("The radius of wire corners. The default is 0.")]
        [Alias("r")]
        public double Radius { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the minimum length of wire segments.
        /// </summary>
        [Description("The minimum length of a wire segment. The default is 10.")]
        [Alias("ml")]
        public double MinimumLength { get; set; } = 10.0;

        /// <summary>
        /// Gets the global coordinates of the points of the wire.
        /// </summary>
        public IReadOnlyList<Vector2> Points => _points.AsReadOnly();

        /// <summary>
        /// Creates a new wire.
        /// </summary>
        /// <param name="name">The name of the wire.</param>
        /// <param name="pinToWire">The pin that will start the wire.</param>
        /// <param name="segments">The wire segments.</param>
        /// <param name="wireToPin">The pin that will end the wire.</param>
        public Wire(string name, PinReference pinToWire, IEnumerable<WireSegmentInfo> segments, PinReference wireToPin)
            : base(name)
        {
            _pinToWire = pinToWire;
            _segments = segments?.ToList() ?? throw new ArgumentNullException(nameof(segments)); ;
            _wireToPin = wireToPin;
        }

        /// <inheritdoc />
        public override PresenceResult Prepare(IPrepareContext context)
        {
            var result = base.Prepare(context);
            if (result == PresenceResult.GiveUp)
                return result;
            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    _localPoints.Clear();
                    _p2w = null;
                    _w2p = null;
                    break;

                case PreparationMode.Find:
                    if (_p2w is null)
                    {
                        _p2w = _pinToWire.GetOrCreate(context.Diagnostics, -1);
                        if (_p2w is not null)
                            _p2w.Connections++;
                    }
                    if (_w2p is null)
                    {
                        _w2p = _wireToPin?.GetOrCreate(context.Diagnostics, 0);
                        if (_w2p is not null)
                            _w2p.Connections++;
                    }
                    break;

                case PreparationMode.Orientation:

                    // Find the pins
                    var p1 = _p2w as IOrientedPin;
                    var p2 = _w2p as IOrientedPin;

                    if (_segments.Count == 1 && _segments[0].Orientation.X.IsZero() && _segments[0].Orientation.Y.IsZero())
                    {
                        // We have a wire that connects two pins but does not have a defined orientation yet
                        // This piece of code will pass this orientation to other nodes if they can be resolved
                        if (p1 == null && p2 == null)
                        {
                            GenerateError(context.Diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Drawable.Name);
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
                                p2.ResolveOrientation(-p1.Orientation, _segments[0].Source, context.Diagnostics);
                            else if (p2f)
                                // The second pin is fixed, so let's use its orientation to set the first one
                                p1.ResolveOrientation(-p2.Orientation, _segments[0].Source, context.Diagnostics);
                            else
                            {
                                // Both aren't fixed, so we might just not have enough information...
                                if (context.Desparateness == DesperatenessLevel.GiveUp)
                                {
                                    GenerateError(context.Diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Drawable.Name);
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
                            if (context.Desparateness == DesperatenessLevel.GiveUp)
                            {
                                GenerateError(context.Diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Drawable.Name);
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
                            if (context.Desparateness == DesperatenessLevel.GiveUp)
                            {
                                GenerateError(context.Diagnostics, _wireToPin, ErrorCodes.AmbiguousOrientation, _w2p.Name, _wireToPin.Drawable.Name);
                                return PresenceResult.GiveUp;
                            }
                            else
                                return PresenceResult.Incomplete;
                        }
                    }
                    else
                    {
                        // In case we have multiple wires, we might have some boundary conditions that we need to be taking care of
                        for (int i = 0; i < _segments.Count; i++)
                        {
                            var segment = _segments[i];
                            if (segment.Orientation.X.IsZero() && segment.Orientation.Y.IsZero())
                            {
                                if (i == 0)
                                {
                                    // Check with first pin, and only with first pin...
                                    if (p1 == null)
                                    {
                                        GenerateError(context.Diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Drawable.Name);
                                        return PresenceResult.GiveUp;
                                    }
                                }
                                else if (i == _segments.Count - 1)
                                {
                                    if (p2 == null)
                                    {
                                        GenerateError(context.Diagnostics, _wireToPin, ErrorCodes.AmbiguousOrientation, _w2p.Name, _wireToPin.Drawable.Name);
                                        return PresenceResult.GiveUp;
                                    }
                                }
                                else
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.UndefinedWireSegment);
                                    return PresenceResult.GiveUp;
                                }
                            }
                        }
                    }
                    break;

                case PreparationMode.Offsets:

                    // Short wire ends to the correct pins
                    if (_p2w is not null)
                    {
                        switch (Helpers.PrepareEntryOffset(_segments[0].Source, _p2w, StartX, StartY, Parser.Nodes.VirtualChainConstraints.XY, context))
                        {
                            case PresenceResult.GiveUp: return PresenceResult.GiveUp;
                            case PresenceResult.Incomplete: result = PresenceResult.Incomplete; break;
                        }
                    }
                    if (_w2p is not null)
                    {
                        switch (Helpers.PrepareEntryOffset(_segments[^1].Source, _w2p, EndX, EndY, Parser.Nodes.VirtualChainConstraints.XY, context))
                        {
                            case PresenceResult.GiveUp: return PresenceResult.GiveUp;
                            case PresenceResult.Incomplete: result = PresenceResult.Incomplete; break;
                        }
                    }
                    switch (Helpers.PrepareSegmentsOffset(
                        GetXName, GetYName, GetOrientation,
                        _segments, Parser.Nodes.VirtualChainConstraints.XY, context))
                    {
                        case PresenceResult.GiveUp: return PresenceResult.GiveUp;
                        case PresenceResult.Incomplete: result = PresenceResult.Incomplete; break;
                    }
                    break;

                case PreparationMode.Groups:
                    for (int i = 0; i < _segments.Count; i++)
                    {
                        // Ignore any unconstrained segments
                        var orientation = GetOrientation(i);
                        if (double.IsNaN(orientation.X) || double.IsNaN(orientation.Y))
                            continue;

                        // Group any other segments
                        switch (Helpers.PrepareSegmentGroup(StartX, StartY, GetXName(i), GetYName(i), Parser.Nodes.VirtualChainConstraints.XY, context))
                        {
                            case PresenceResult.GiveUp: return PresenceResult.GiveUp;
                            case PresenceResult.Incomplete: result = PresenceResult.Incomplete; break;
                        }
                    }
                    break;

                case PreparationMode.DrawableGroups:
                    context.GroupDrawableTo(this, StartX, StartY);
                    break;
            }
            
            return result;
        }

        /// <inheritdoc />
        public override void Update(IUpdateContext context)
        {
            _localPoints.Clear();

            // Extract the first node
            var last = context.GetValue(StartX, StartY);
            int count = context.WireSegments.Count;
            _localPoints.Add(new(last, false));
            for (int i = 0; i < _segments.Count; i++)
            {
                var next = context.GetValue(GetXName(i), GetYName(i));

                // Add jump-over points if specified
                if (Variants.Contains(JumpOver))
                    AddJumpOverWires(last, next, context.WireSegments.Take(count));

                _localPoints.Add(new(next, false));
                context.WireSegments.Add(new(last, next));
                if (_localPoints.Count > 2)
                    count++;
                last = next;
            }
        }

        private void AddJumpOverWires(Vector2 last, Vector2 next, IEnumerable<WireSegment> segments)
        {
            // Calculate overlapping vectors
            var d = last - next;
            SortedDictionary<double, Vector2> pts = [];
            foreach (var segment in segments)
            {
                var sd = segment.Start - segment.End;
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
                if (tn <= tol || un <= -tol)
                    continue;
                if (tn >= denom * 0.999 || un >= denom * 1.001)
                    continue;
                var intersection = last - tn / denom * d;
                sd = intersection - last;
                double distance = sd.X * sd.X + sd.Y * sd.Y;
                if (!pts.ContainsKey(distance))
                    pts.Add(distance, intersection);
            }
            if (pts.Count > 0)
            {
                double minDist = _jumpOverRadius * _jumpOverRadius;
                double maxDist = (d.X * d.X + d.Y * d.Y) - minDist;
                double lastDist = double.NegativeInfinity;
                foreach (var pair in pts)
                {
                    if (pair.Key >= minDist && pair.Key <= maxDist)
                    {
                        if (pair.Key >= lastDist + 2 * _jumpOverRadius)
                        {
                            _localPoints.Add(new(pair.Value, true));
                            lastDist = pair.Key;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void Register(IRegisterContext context)
        {
            var fromX = context.GetOffset(StartX);
            var fromY = context.GetOffset(StartY);
            for (int i = 0; i < _segments.Count; i++)
            {
                string x = GetXName(i);
                string y = GetYName(i);
                var toX = context.GetOffset(x);
                var toY = context.GetOffset(y);
                var segment = _segments[i];
                var orientation = GetOrientation(i);
                double length = segment.Length < 0 ? MinimumLength : segment.Length;

                // Ignore unconstrained wires
                if (double.IsNaN(orientation.X) || double.IsNaN(orientation.Y))
                    continue;

                // Wire is of minimum length
                if (orientation.X.IsZero() && !StringComparer.Ordinal.Equals(fromY.Representative, toY.Representative))
                    MinimumConstraint.AddDirectionalMinimum(context.Circuit, y, fromY, toY, orientation.Y * length);
                else if (orientation.Y.IsZero() && !StringComparer.Ordinal.Equals(fromX.Representative, toX.Representative))
                    MinimumConstraint.AddDirectionalMinimum(context.Circuit, x, fromX, toX, orientation.X * length);
                else if (!orientation.X.IsZero() && !orientation.Y.IsZero() && !StringComparer.Ordinal.Equals(fromX.Representative, toX.Representative))
                    MinimumConstraint.AddDirectionalMinimum(context.Circuit, x, fromX, fromY, toX, toY, orientation, length);
                fromX = toX;
                fromY = toY;
            }
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            List<Marker> markers = [];
            if (!Variants.Contains(Hidden) && _localPoints.Count > 0)
            {
                // Compute the graphical style
                var tf = builder.CurrentTransform;
                _points.Clear();
                builder.Path(builder =>
                {
                    builder.MoveTo(_localPoints[0].Location);
                    _points.Add(tf.Apply(_localPoints[0].Location));
                    int segment = 0;
                    var startMarkers = _segments[0].StartMarkers;
                    var last = _localPoints[0].Location;
                    for (int i = 1; i < _localPoints.Count; i++)
                    {
                        var current = _localPoints[i].Location;

                        // Draw a small half circle for crossing over this point
                        if (_localPoints[i].IsJumpOver)
                        {
                            GetNewAxes(last, current, out var nx, out var ny);
                            var s = current - nx * _jumpOverRadius;
                            var e = current + _jumpOverRadius * nx;
                            var m = current + ny * _jumpOverRadius;

                            builder.LineTo(s);

                            // Deal with the start marker
                            if (startMarkers != null)
                            {
                                foreach (var marker in startMarkers)
                                {
                                    marker.Location = builder.Start;
                                    marker.Orientation = -builder.StartNormal; // We will invert the direction to align with the nature of circuits and diagrams
                                    markers.Add(marker);
                                    startMarkers = null;
                                }
                            }

                            nx *= 0.55 * _jumpOverRadius;
                            ny *= 0.55 * _jumpOverRadius;
                            builder.CurveTo(s + ny, m - nx, m);
                            builder.SmoothTo(e + ny, e);
                        }
                        else
                        {
                            _points.Add(tf.Apply(_localPoints[i].Location));
                            if (Radius.IsZero() || i >= _localPoints.Count - 1)
                                builder.LineTo(current);
                            else
                            {
                                var nu = last - current;
                                double lu = nu.Length;
                                var nv = _localPoints[i + 1].Location - current;
                                double lv = nv.Length;
                                if (lu > 0 && lv > 0.0)
                                {
                                    nu /= lu;
                                    nv /= lv;
                                    double dot = nu.Dot(nv);
                                    if (dot > 0.999 || dot < -0.999)
                                        builder.LineTo(current);
                                    else
                                    {
                                        // Rounded corner
                                        double x = Radius / Math.Tan(Math.Acos(dot) * 0.5);
                                        if (x > lu * 0.5 || x > lv * 0.5)
                                        {
                                            // No place, just do straight line again
                                            builder.LineTo(current);
                                        }
                                        else
                                        {
                                            // Segments
                                            builder.LineTo(current + nu * x);
                                            builder.ArcTo(Radius, Radius, 0.0, false, nu.X * nv.Y - nu.Y * nv.X < 0.0, current + nv * x);
                                        }
                                    }
                                }
                                else 
                                    builder.LineTo(current); // We can't fit a curve
                            }

                            // Deal with the start marker
                            if (startMarkers != null)
                            {
                                foreach (var marker in startMarkers)
                                {
                                    marker.Location = builder.Start;
                                    marker.Orientation = -builder.StartNormal; // We will invert the direction to align with the nature of circuits and diagrams
                                    markers.Add(marker);
                                    startMarkers = null;
                                }
                            }

                            // Deal with the end marker
                            var endMarkers = _segments[segment].EndMarkers;
                            if (endMarkers != null)
                            {
                                foreach (var marker in endMarkers)
                                {
                                    marker.Location = builder.End;
                                    marker.Orientation = builder.EndNormal;
                                    markers.Add(marker);
                                }
                            }
                            segment++;

                            // Prepare for the next start marker
                            if (segment < _segments.Count)
                                startMarkers = _segments[segment].StartMarkers;
                        }

                        last = current;
                    }
                }, style);

                // Draw the markers (if any)
                foreach (var marker in markers)
                    marker?.Draw(builder, style);
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
        private Vector2 GetOrientation(int index)
        {
            var orientation = _segments[index].Orientation;
            if (orientation.X.IsZero() && orientation.Y.IsZero())
            {
                // Take the pin orientation instead
                if (index == 0 && _p2w is IOrientedPin p1)
                    orientation = p1.Orientation;
                else if (index == _segments.Count - 1 && _w2p is IOrientedPin p2)
                    orientation = -p2.Orientation;
                else
                    orientation = new(1, 0);
            }
            return orientation;
        }
        private void GenerateError(IDiagnosticHandler diagnostics, PinReference pin, ErrorCodes code, params object[] arguments)
        {
            if (pin.Name.Length > 0)
                diagnostics?.Post(pin.Source, code, arguments);
            else
                diagnostics?.Post(pin.Drawable.Sources.FirstOrDefault(), code, arguments);
        }
    }
}
