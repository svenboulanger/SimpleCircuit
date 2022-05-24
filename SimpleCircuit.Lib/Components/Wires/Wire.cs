using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire that can have a variable length.
    /// </summary>
    public class Wire : Drawable
    {
        private readonly WireInfo _info;
        private readonly List<Vector2> _vectors = new();

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
            DrawingVariants = Variant.Do(DrawWire);
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
        public override void Update(IBiasingSimulationState state, CircuitContext context, IDiagnosticHandler diagnostics)
        {
            _vectors.Clear();

            double x = 0.0, y = 0.0;
            if (state.TryGetValue(context.Nodes.Shorts[StartX], out var solX))
                x = solX.Value;
            if (state.TryGetValue(context.Nodes.Shorts[StartY], out var solY))
                y = solY.Value;
            _vectors.Add(new(x, y));

            for (int i = 0; i < _info.Segments.Count; i++)
            {
                x = 0.0;
                y = 0.0;
                if (state.TryGetValue(context.Nodes.Shorts[GetXName(i)], out solX))
                    x = solX.Value;
                if (state.TryGetValue(context.Nodes.Shorts[GetYName(i)], out solY))
                    y = solY.Value;
                _vectors.Add(new(x, y));
            }
        }

        /// <inheritdoc />
        public override void Register(CircuitContext context, IDiagnosticHandler diagnostics)
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

        private void RegisterWire(CircuitContext context, string x, string y, string tx, string ty, WireSegment segment)
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

        private void DrawWire(SvgDrawing drawing)
        {
            if (_info.IsVisible && _vectors.Count > 0)
            {
                drawing.Path(builder =>
                {
                        // Start the first point and build the path
                    builder.MoveTo(_vectors[0]);
                    for (int i = 1; i < _vectors.Count; i++)
                        builder.LineTo(_vectors[i]);
                }, _info.Options);
            }
        }

        private string GetXName(int index) => $"{Name}.{index + 1}.x";
        private string GetYName(int index) => $"{Name}.{index + 1}.y";
    }
}
