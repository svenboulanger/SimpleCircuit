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
                    diagnostics?.Post(ErrorCodes.CouldNotConstrainOrientation, Name);
                    return false;
                }
                return true;
            }
        }

        /// <inheritdoc />
        public override void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            var offset = _origin is ITransformingDrawable tfd ? tfd.TransformOffset(Offset) : Offset;
            switch (context.Mode)
            {
                case NodeRelationMode.Offsets:
                    context.Offsets.Group(_origin.X, X, offset.X);
                    context.Offsets.Group(_origin.Y, Y, offset.Y);
                    break;

                default:
                    return;
            }
        }

        /// <summary>
        /// Convert to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"{Owner.Name}[{Name}]";
    }
}
