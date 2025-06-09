using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A pin that has a position and an orientation.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="FixedOrientedPin"/>.
    /// </remarks>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="relativeOffset">The relative offset.</param>
    /// <param name="relativeOrientation">The relative orientation.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="owner"/> is <c>null</c>.</exception>
    public class FixedOrientedPin(string name, string description, ILocatedDrawable owner, Vector2 relativeOffset, Vector2 relativeOrientation)
        : Pin(name, description, owner), IOrientedPin
    {
        private readonly ILocatedPresence _origin = owner;

        /// <inheritdoc />
        ILocatedDrawable IPin.Owner => Owner;

        /// <summary>
        /// Gets or sets the offset of the pin relative to its owner.
        /// </summary>
        public Vector2 Offset { get; set; } = relativeOffset;

        /// <summary>
        /// Gets or sets the orientation of the pin relative to its owner.
        /// </summary>
        public Vector2 RelativeOrientation { get; set; } = relativeOrientation / relativeOrientation.Length;

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

        /// <inheritdoc />
        public bool ResolveOrientation(Vector2 orientation, TextLocation source, IDiagnosticHandler diagnostics)
        {
            // Make sure the orientation is normalized to avoid issues...
            orientation /= orientation.Length;

            // Constrain the owner orientation
            if (Owner is IOrientedDrawable od)
                return od.ConstrainOrientation(RelativeOrientation, orientation, source, diagnostics);
            else
            {
                var error = orientation - RelativeOrientation;
                if (!error.X.IsZero() || !error.Y.IsZero())
                {
                    diagnostics?.Post(source, ErrorCodes.CouldNotConstrainOrientation, Name);
                    return false;
                }
                return true;
            }
        }

        /// <inheritdoc />
        public override void Register(IRegisterContext context)
        {
        }

        /// <inheritdoc />
        public override PresenceResult Prepare(IPrepareContext context)
        {
            switch (context.Mode)
            {
                case PreparationMode.Offsets:
                    var offset = _origin is ITransformingDrawable tfd ? tfd.TransformOffset(Offset) : Offset;
                    if (!context.Offsets.Group(_origin.X, X, offset.X))
                    {
                        context.Diagnostics?.Post(ErrorCodes.CannotResolveFixedOffsetFor, offset.X, Name);
                        return PresenceResult.GiveUp;
                    }
                    if (!context.Offsets.Group(_origin.Y, Y, offset.Y))
                    {
                        context.Diagnostics?.Post(ErrorCodes.CannotResolveFixedOffsetFor, offset.Y, Name);
                        return PresenceResult.GiveUp;
                    }
                    break;
            }
            return PresenceResult.Success;
        }

        /// <summary>
        /// Convert to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"{Owner.Name}[{Name}]";
    }
}
