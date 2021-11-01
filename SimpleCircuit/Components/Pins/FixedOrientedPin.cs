using SimpleCircuit.Diagnostics;
using System;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A pin that has a position.
    /// </summary>
    public class FixedOrientedPin : Pin, IOrientedPin
    {
        private readonly ILocatedPresence _origin;

        /// <inheritdoc/>
        public new IOrientedDrawable Owner { get; }

        /// <inheritdoc />
        ILocatedDrawable IPin.Owner => Owner;

        /// <inheritdoc />
        public Vector2 Offset { get; set; }

        /// <inheritdoc />
        public Vector2 LocalOrientation { get; }

        /// <inheritdoc />
        public Vector2 Orientation => _origin is ITransformingDrawable tfd ? tfd.TransformNormal(LocalOrientation) : LocalOrientation;

        public FixedOrientedPin(string name, string description, IOrientedDrawable owner, Vector2 relativeOffset, Vector2 relativeOrientation)
            : base(name, description, owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Offset = relativeOffset;
            LocalOrientation = relativeOrientation / relativeOrientation.Length;
            _origin = owner;
        }

        /// <inheritdoc />
        public bool ResolveOrientation(Vector2 orientation, IDiagnosticHandler diagnostics)
        {
            orientation /= orientation.Length;
            return Owner.ConstrainOrientation(LocalOrientation, orientation, diagnostics);
        }

        /// <inheritdoc />
        public override void Register(CircuitContext context, IDiagnosticHandler diagnostics)
        {
            // Our pin is relative to the owner's location, so we need to create an offset for that!
            var map = context.Nodes.Shorts;
            string x = map[X];
            string ox = map[_origin.X];
            string y = map[Y];
            string oy = map[_origin.Y];
            var offset = _origin is ITransformingDrawable tfd ? tfd.TransformOffset(Offset) : Offset;
            offset = offset.Order(ref ox, ref x, ref oy, ref y);

            // Apply locations according to this offset!
            if (x != ox)
                OffsetConstraint.AddOffset(context.Circuit, X, ox, x, offset.X);
            if (y != oy)
                OffsetConstraint.AddOffset(context.Circuit, Y, oy, y, offset.Y);
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            // Register shorts
            var offset = _origin is ITransformingDrawable tfd ? tfd.TransformOffset(Offset) : Offset;
            if (offset.X.IsZero())
                context.Shorts.Group(X, _origin.X);
            if (offset.Y.IsZero())
                context.Shorts.Group(Y, _origin.Y);
        }

        /// <summary>
        /// Convert to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"{Owner.Name}[{Name}]";
    }
}
