using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A pin with a loose oriententation (the orientation is set by whoever tries setting it).
    /// </summary>
    public class LooselyOrientedPin : Pin, IOrientedPin
    {
        private readonly ILocatedDrawable _origin;

        /// <inheritdoc />
        public bool HasFixedOrientation { get; private set; }

        /// <inheritdoc />
        public Vector2 Orientation { get; private set; }

        /// <inheritdoc />
        public bool HasFreeOrientation => !HasFixedOrientation;

        /// <summary>
        /// Gets or sets the offset of the pin relative to its owner.
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Creates an loosely oriented pin. This means that the pin can still be oriented
        /// but its location can also be fixed.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="owner"></param>
        public LooselyOrientedPin(string name, string description, ILocatedDrawable owner)
            : base(name, description, owner)
        {
            _origin = owner;
        }

        /// <inheritdoc />
        public override bool DiscoverNodeRelationships(IRelationshipContext context)
        {
            var offset = _origin is ITransformingDrawable tfd ? tfd.TransformOffset(Offset) : Offset;
            switch (context.Mode)
            {
                case NodeRelationMode.Offsets:
                    if (!context.Offsets.Group(_origin.X, X, offset.X))
                    {
                        context.Diagnostics?.Post(ErrorCodes.CannotResolveFixedOffsetFor, offset.X, Name);
                        return false;
                    }
                    if (!context.Offsets.Group(_origin.Y, Y, offset.Y))
                    {
                        context.Diagnostics?.Post(ErrorCodes.CannotResolveFixedOffsetFor, offset.Y, Name);
                        return false;
                    }
                    break;
            }
            return true;
        }

        /// <inheritdoc />
        public override void Register(IRegisterContext context)
        {
        }

        /// <inheritdoc />
        public bool ResolveOrientation(Vector2 orientation, Token source, IDiagnosticHandler diagnostics)
        {
            if (HasFixedOrientation)
            {
                // We cannot change the orientation after it has already been determined
                if (orientation.Dot(Orientation) < 0.999)
                {
                    diagnostics?.Post(source, ErrorCodes.CouldNotConstrainOrientation, Name);
                    return false;
                }
            }
            else
            {
                // We are not being difficult, just give the orientation it wants...
                HasFixedOrientation = true;
                Orientation = orientation;
            }
            return true;
        }

        /// <summary>
        /// Convert to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"{Owner.Name}[{Name}]";
    }
}
