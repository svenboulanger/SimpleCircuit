using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Markers;
using SimpleCircuit.Parser;
using SpiceSharp.Components;
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
        private IPin _w2p, _p2w;
        private readonly PinInfo _wireToPin, _pinToWire;
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

        /// <summary>
        /// The variant used for a wire that is dashed.
        /// </summary>
        public const string Dashed = "dashed";

        /// <summary>
        /// The variant used for a wire that is dotted.
        /// </summary>
        public const string Dotted = "dotted";

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

        [Description("The radius.")]
        [Alias("r")]
        public double RoundRadius { get; set; } = 0.0;

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
        public Wire(string name, PinInfo pinToWire, IEnumerable<WireSegmentInfo> segments, PinInfo wireToPin)
            : base(name)
        {
            _pinToWire = pinToWire;
            _segments = segments?.ToList() ?? throw new ArgumentNullException(nameof(segments)); ;
            _wireToPin = wireToPin;
        }

        /// <inheritdoc />
        public override bool Reset(IResetContext context)
        {
            if (!base.Reset(context))
                return false;
            _localPoints.Clear();
            _p2w = null;
            _w2p = null;
            return true;
        }

        /// <inheritdoc />
        public override PresenceResult Prepare(IPrepareContext context)
        {
            if (context.Mode == PreparationMode.Offsets)
            {
                // Find the pins
                _p2w = _pinToWire?.GetOrCreate(context.Diagnostics, -1);
                _w2p = _wireToPin?.GetOrCreate(context.Diagnostics, 0);

                // Make sure these pins know they are being connected to
                if (_p2w != null)
                    _p2w.Connections++;
                if (_w2p != null)
                    _w2p.Connections++;

                var p1 = _p2w as IOrientedPin;
                var p2 = _w2p as IOrientedPin;

                if (_segments.Count == 1 && _segments[0].Orientation.X.IsZero() && _segments[0].Orientation.Y.IsZero() && !_segments[0].IsUnconstrained)
                {
                    // We have a wire that connects two pins but does not have a defined orientation yet
                    // This piece of code will pass this orientation to other nodes if they can be resolved
                    if (p1 == null && p2 == null)
                    {
                        GenerateError(context.Diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Component.Fullname);
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
                                GenerateError(context.Diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Component.Fullname);
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
                            GenerateError(context.Diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Component.Fullname);
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
                            GenerateError(context.Diagnostics, _wireToPin, ErrorCodes.AmbiguousOrientation, _w2p.Name, _wireToPin.Component.Fullname);
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
                        if (segment.IsUnconstrained)
                            continue;
                        else if (segment.Orientation.X.IsZero() && segment.Orientation.Y.IsZero())
                        {
                            if (i == 0)
                            {
                                // Check with first pin, and only with first pin...
                                if (p1 == null)
                                {
                                    GenerateError(context.Diagnostics, _pinToWire, ErrorCodes.AmbiguousOrientation, _p2w.Name, _pinToWire.Component.Fullname);
                                    return PresenceResult.GiveUp;
                                }
                            }
                            else if (i == _segments.Count - 1)
                            {
                                if (p2 == null)
                                {
                                    GenerateError(context.Diagnostics, _wireToPin, ErrorCodes.AmbiguousOrientation, _w2p.Name, _wireToPin.Component.Fullname);
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
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public override bool DiscoverNodeRelationships(IRelationshipContext context)
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
                            context.Diagnostics?.Post(_segments[0].Source, ErrorCodes.CannotAlignAlongX, _p2w.X, StartX);
                            return false;
                        }
                        if (!context.Offsets.Group(_p2w.Y, StartY, 0.0))
                        {
                            context.Diagnostics?.Post(_segments[0].Source, ErrorCodes.CannotAlignAlongY, _p2w.Y, StartY);
                            return false;
                        }
                    }
                    if (_w2p != null)
                    {
                        if (!context.Offsets.Group(_w2p.X, EndX, 0.0))
                        {
                            context.Diagnostics?.Post(_segments[^1].Source, ErrorCodes.CannotAlignAlongX, _w2p.X, EndX);
                            return false;
                        }
                        if (!context.Offsets.Group(_w2p.Y, EndY, 0.0))
                        {
                            context.Diagnostics?.Post(_segments[^1].Source, ErrorCodes.CannotAlignAlongY, _w2p.Y, EndY);
                            return false;
                        }
                    }

                    // Deal with horizontal and vertical segments
                    x = StartX;
                    y = StartY;
                    for (int i = 0; i < _segments.Count; i++)
                    {
                        string tx = GetXName(i);
                        string ty = GetYName(i);
                        var segment = _segments[i];

                        // Ignore unconstrained wires
                        if (!segment.IsUnconstrained)
                        {
                            var orientation = GetOrientation(i);
                            if (segment.IsFixed)
                            {
                                double l = segment.Length;

                                if (!context.Offsets.Group(x, tx, orientation.X * l))
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(orientation.X * l), segment.Source.Content);
                                    return false;
                                }
                                if (!context.Offsets.Group(y, ty, orientation.Y * l))
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(orientation.Y * l), segment.Source.Content);
                                    return false;
                                }
                            }
                            else
                            {
                                if (orientation.X.IsZero())
                                {
                                    if (!context.Offsets.Group(x, tx, 0.0))
                                    {
                                        context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotAlignAlongX, x, tx);
                                        return false;
                                    }
                                }
                                if (orientation.Y.IsZero())
                                {
                                    if (!context.Offsets.Group(y, ty, 0.0))
                                    {
                                        context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotAlignAlongY, y, ty);
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
                    for (int i = 0; i < _segments.Count; i++)
                    {
                        var toOffsetX = context.Offsets[GetXName(i)];
                        var toOffsetY = context.Offsets[GetYName(i)];
                        var segment = _segments[i];
                        if (!segment.IsUnconstrained && !segment.IsFixed)
                        {
                            var orientation = GetOrientation(i);

                            // If the wire is length is fixed, then we just need to compare their relative offset
                            if (!orientation.X.IsZero())
                            {
                                if (!MinimumConstraint.MinimumDirectionalLink(context, offsetX, toOffsetX, orientation.X * segment.Length))
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CouldNotSatisfyMinimumOfForInX, Math.Abs(orientation.X * segment.Length), segment.Source.Content);
                            }
                            if (!orientation.Y.IsZero())
                            {
                                if (!MinimumConstraint.MinimumDirectionalLink(context, offsetY, toOffsetY, orientation.Y * segment.Length))
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CouldNotSatisfyMinimumOfForInY, Math.Abs(orientation.Y * segment.Length), segment.Source.Content);
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

        /// <inheritdoc />
        public override void Update(IUpdateContext context)
        {
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
            Vector2 d = last - next;
            SortedDictionary<double, Vector2> pts = [];
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
                if (tn <= tol || un <= -tol)
                    continue;
                if (tn >= denom * 0.999 || un >= denom * 1.001)
                    continue;
                Vector2 intersection = last - tn / denom * d;
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
            var map = context.Relationships.Offsets;
            var fromX = map[StartX];
            var fromY = map[StartY];
            for (int i = 0; i < _segments.Count; i++)
            {
                string x = GetXName(i);
                string y = GetYName(i);
                var toX = map[x];
                var toY = map[y];
                var segment = _segments[i];
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
            List<Marker> markers = [];
            if (!Variants.Contains(Hidden) && _localPoints.Count > 0)
            {
                if (Variants.Contains(Dashed))
                    drawing.RequiredCSS.Add(".dashed { stroke-dasharray: 2 2; }");
                if (Variants.Contains(Dotted))
                    drawing.RequiredCSS.Add(".dotted { stroke-dasharray: 0.5 2; }");
                GraphicOptions options = Variants.Select(Dashed, Dotted) switch
                {
                    0 => new("wire", "dashed"),
                    1 => new("wire", "dotted"),
                    _ => new("wire"),
                };

                var tf = drawing.CurrentTransform;
                drawing.Path(builder =>
                {
                    builder.MoveTo(_localPoints[0].Location);
                    _points.Add(tf.Apply(_localPoints[0].Location));
                    int segment = 0;
                    var startMarkers = _segments[0].StartMarkers;
                    Vector2 last = _localPoints[0].Location;
                    for (int i = 1; i < _localPoints.Count; i++)
                    {
                        Vector2 current = _localPoints[i].Location;

                        // Draw a small half circle for crossing over this point
                        if (_localPoints[i].IsJumpOver)
                        {
                            GetNewAxes(last, current, out var nx, out var ny);
                            Vector2 s = current - nx * _jumpOverRadius;
                            Vector2 e = current + _jumpOverRadius * nx;
                            Vector2 m = current + ny * _jumpOverRadius;

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
                            if (RoundRadius.IsZero() || i >= _localPoints.Count - 1)
                                builder.LineTo(current);
                            else
                            {
                                Vector2 nu = last - current;
                                double lu = nu.Length;
                                Vector2 nv = _localPoints[i + 1].Location - current;
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
                                        double x = RoundRadius / Math.Tan(Math.Acos(dot) * 0.5);
                                        if (x > lu * 0.5 || x > lv * 0.5)
                                        {
                                            // No place, just do straight line again
                                            builder.LineTo(current);
                                        }
                                        else
                                        {
                                            // Segments
                                            builder.LineTo(current + nu * x);
                                            builder.ArcTo(RoundRadius, RoundRadius, 0.0, false, nu.X * nv.Y - nu.Y * nv.X < 0.0, current + nv * x);
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
                }, options);

                // Draw the markers (if any)
                foreach (var marker in markers)
                    marker?.Draw(drawing);
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
            if (pin.Name.Content.Length > 0)
                diagnostics?.Post(pin.Name, code, arguments);
            else
                diagnostics?.Post(pin.Component.Source, code, arguments);
        }
    }
}
