using SimpleCircuit.Diagnostics;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using System;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A pin that is a minimum distance from a reference point.
    /// </summary>
    public class MinimumOffsetPin : Pin
    {
        private readonly ILocatedPresence _origin;

        /// <summary>
        /// Gets the owner of the pin.
        /// </summary>
        public new IOrientedDrawable Owner { get; }

        /// <summary>
        /// Gets or sets a flag whether the offset given by <see cref="MinimumOffset"/> is fixed instead of a minimum.
        /// </summary>
        public bool Fix { get; set; } = false;

        /// <summary>
        /// Gets the (local) direction of the minimum relative pin.
        /// </summary>
        public Vector2 Direction { get; }

        /// <summary>
        /// Gets the minimum offset.
        /// </summary>
        public double MinimumOffset { get; set; }

        public MinimumOffsetPin(string name, string description, IOrientedDrawable owner, Vector2 direction, double minimum)
            : this(name, description, owner, owner, direction, minimum)
        {
        }

        public MinimumOffsetPin(string name, string description, IOrientedDrawable owner, ILocatedPresence origin, Vector2 direction, double minimumOffset)
            : base(name, description, owner)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Owner = owner;
            Direction = direction;
            MinimumOffset = minimumOffset;
            _origin = origin ?? owner;
        }

        /// <inheritdoc />
        public override void Register(CircuitContext context, IDiagnosticHandler diagnostics)
        {
            if (Fix)
            {
                RegisterFixed(context, diagnostics);
                return;
            }

            var map = context.Nodes.Shorts;
            var ckt = context.Circuit;
            string x = map[X];
            string ox = map[_origin.X];
            string y = map[Y];
            string oy = map[_origin.Y];
            var direction = _origin is ITransformingDrawable tfd ? tfd.TransformNormal(Direction) : Direction;
            direction = direction.Order(ref ox, ref x, ref oy, ref y);

            // If we only work along one axis, we can simplify the schematic
            if (x == ox)
            {
                MinimumConstraint.AddMinimum(ckt, Y, oy, y, MinimumOffset);
                return;
            }
            if (y == oy)
            {
                MinimumConstraint.AddMinimum(ckt, X, ox, x, MinimumOffset);
                return;
            }

            // General case, both X and Y are different
            AddControlledMinimum(ckt, $"{Owner.Name}[{Name}]", ox, x, oy, y, direction);
            MinimumConstraint.AddMinimum(ckt, $"{Owner.Name}[{Name}].min.x", ox, x, direction.X * MinimumOffset);
            MinimumConstraint.AddMinimum(ckt, $"{Owner.Name}[{Name}].min.y", oy, y, direction.Y * MinimumOffset);
        }

        private void RegisterFixed(CircuitContext context, IDiagnosticHandler diagnostics)
        {
            // No need to go through all these difficult things, let's just apply directly
            var map = context.Nodes.Shorts;
            string x = map[X];
            string ox = map[_origin.X];
            string y = map[Y];
            string oy = map[_origin.Y];
            var direction = _origin is ITransformingDrawable tfd ? tfd.TransformNormal(Direction) : Direction;
            direction = direction.Order(ref ox, ref x, ref oy, ref y);
            if (x != ox)
                OffsetConstraint.AddOffset(context.Circuit, X, ox, x, direction.X * MinimumOffset);
            if (y != oy)
                OffsetConstraint.AddOffset(context.Circuit, Y, oy, y, direction.Y * MinimumOffset);
        }

        private static void AddControlledMinimum(IEntityCollection ckt, string name,  string lowX, string highX, string lowY, string highY, Vector2 direction)
        {
            string i = $"{name}.xc";
            MinimumConstraint.AddRectifyingElement(ckt, $"D{i}", i, highX);
            ckt.Add(new VoltageControlledVoltageSource($"E{i}", i, lowX, highY, lowY, direction.X / direction.Y));

            i = $"{name}.yc";
            MinimumConstraint.AddRectifyingElement(ckt, $"D{i}", i, highY);
            ckt.Add(new VoltageControlledVoltageSource($"E{i}", i, lowY, highX, lowX, direction.Y / direction.X));
        }

        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            // Deal with shorts
            var direction = _origin is ITransformingDrawable tfd ? tfd.TransformNormal(Direction) : Direction;
            if (direction.X.IsZero())
                context.Shorts.Group(X, _origin.X);
            if (direction.Y.IsZero())
                context.Shorts.Group(Y, _origin.Y);
        }
    }
}
