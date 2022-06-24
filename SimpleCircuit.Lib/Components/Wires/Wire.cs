using SimpleCircuit.Diagnostics;
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
        /// <param name="info">The wire information.</param>
        public Wire(string name, WireInfo info)
            : base(name)
        {
            _info = info ?? throw new ArgumentNullException(nameof(info));
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            string x = StartX, y = StartY;
            for (int i = 0; i < _info.Segments.Count; i++)
            {
                string tx = GetXName(i), ty = GetYName(i);
                if (_info.Segments[i].Orientation.X.IsZero())
                    context.Shorts.Group(x, tx);
                if (_info.Segments[i].Orientation.Y.IsZero())
                    context.Shorts.Group(y, ty);
                x = tx;
                y = ty;
            }
        }

        /// <inheritdoc />
        public override void Reset()
        {
            _vectors.Clear();
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
                RegisterWire(context, x, y, tx, ty, _info.Segments[i]);
                x = tx;
                y = ty;
            }
        }

        private void RegisterWire(CircuitSolverContext context, string x, string y, string tx, string ty, WireSegmentInfo segment)
        {
            var map = context.Nodes.Shorts;
            string ox = map[x];
            string oy = map[y];
            tx = map[tx];
            ty = map[ty];
            var direction = segment.Orientation.Order(ref ox, ref tx, ref oy, ref ty);
            if (segment.IsFixed)
            {
                if (ox != tx)
                    OffsetConstraint.AddOffset(context.Circuit, x, ox, tx, direction.X * segment.Length);
                if (oy != ty)
                    OffsetConstraint.AddOffset(context.Circuit, y, oy, ty, direction.Y * segment.Length);
            }
            else
            {
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
                        if (_vectors[i].IsJumpOver)
                        {
                            // Draw a small half circle for crossing over this point
                            Vector2 nx = _vectors[i].Location - _vectors[i - 1].Location;
                            if (nx.X.IsZero() && nx.Y.IsZero())
                                continue;
                            nx /= nx.Length;
                            Vector2 ny = new(nx.Y, -nx.X);
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

        private string GetXName(int index) => $"{Name}.{index + 1}.x";
        private string GetYName(int index) => $"{Name}.{index + 1}.y";
    }
}
