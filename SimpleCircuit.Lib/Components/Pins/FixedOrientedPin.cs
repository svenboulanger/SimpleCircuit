using SimpleCircuit.Diagnostics;
using System;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A pin that has a position and an orientation.
    /// </summary>
    public class FixedOrientedPin : Pin, IOrientedPin
    {
        private readonly ILocatedPresence _origin;

        /// <inheritdoc />
        ILocatedDrawable IPin.Owner => Owner;

        /// <summary>
        /// Gets or sets the offset of the pin relative to its owner.
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Gets or sets the orientation of the pin relative to its owner.
        /// </summary>
        public Vector2 RelativeOrientation { get; set; }

        /// <inheritdoc />
        public bool HasFixedOrientation
        {
            get
            {
                if (Owner is IOrientedDrawable od)
                    return od.IsConstrained(RelativeOrientation);
                else
                    return true;
            }
        }

        /// <inheritdoc />
        public bool HasFreeOrientation
        {
            get
            {
                if (Owner is IOrientedDrawable od)
                    return od.OrientationDegreesOfFreedom == 2;
                else
                    return false;
            }
        }

        /// <inheritdoc />
        public Vector2 Orientation => _origin is ITransformingDrawable tfd ? tfd.TransformNormal(RelativeOrientation) : RelativeOrientation;

        /// <summary>
        /// Creates a new <see cref="FixedOrientedPin"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="relativeOffset">The relative offset.</param>
        /// <param name="relativeOrientation">The relative orientation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="owner"/> is <c>null</c>.</exception>
        public FixedOrientedPin(string name, string description, ILocatedDrawable owner, Vector2 relativeOffset, Vector2 relativeOrientation)
            : base(name, description, owner)
        {
            Offset = relativeOffset;
            RelativeOrientation = relativeOrientation / relativeOrientation.Length;
            _origin = owner;
        }

        /// <inheritdoc />
        public bool ResolveOrientation(Vector2 orientation, IDiagnosticHandler diagnostics)
        {
            // Make sure the orientation is normalized to avoid issues...
            orientation /= orientation.Length;

            // Constrain the owner orientation
            if (Owner is IOrientedDrawable od)
                return od.ConstrainOrientation(RelativeOrientation, orientation, diagnostics);
            else
            {
                var error = orientation - RelativeOrientation;
                if (!error.X.IsZero() || !error.Y.IsZero())
                {
                    diagnostics?.Post(ErrorCodes.CouldNotConstrainPin, Name);
                    return false;
                }
                return true;
            }
        }

        /// <inheritdoc />
        public override void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
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

            // Link the pin to its owner
            context.Relative.Group(X, _origin.X);
            context.Relative.Group(Y, _origin.Y);
        }

        /// <summary>
        /// Convert to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"{Owner.Name}[{Name}]";
    }
}
