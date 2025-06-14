using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A pin without orientation, at a fixed position.
    /// </summary>
    /// <remarks>
    /// Creates a pin at a fixed relative position.
    /// </remarks>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="origin">The origin.</param>
    /// <param name="offset">The offset.</param>
    public class FixedPin(string name, string description, ILocatedDrawable owner, ILocatedPresence origin, Vector2 offset) : Pin(name, description, owner)
    {
        private readonly ILocatedPresence _origin = origin;

        /// <summary>
        /// Gets or sets the local offset of the pin. This does not include any modifications by the 
        /// </summary>
        public Vector2 Offset { get; set; } = offset;

        /// <summary>
        /// Creates a pin at a fixed relative position.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="offset">The offset relative to the owner position.</param>
        public FixedPin(string name, string description, ILocatedDrawable owner, Vector2 offset)
            : this(name, description, owner, owner, offset)
        {
        }

        /// <inheritdoc />
        public override PresenceResult Prepare(IPrepareContext context)
        {

            switch (context.Mode)
            {
                case PreparationMode.Offsets:
                    Vector2 offset = _origin is ITransformingDrawable tfd ? tfd.TransformOffset(Offset) : Offset;
                    if (!context.Offsets.Group(_origin.X, X, offset.X))
                    {
                        context.Diagnostics?.Post(ErrorCodes.CouldNotResolveFixedOffsetFor, offset.X, Name);
                        return PresenceResult.GiveUp;
                    }
                    if (!context.Offsets.Group(_origin.Y, Y, offset.Y))
                    {
                        context.Diagnostics?.Post(ErrorCodes.CouldNotResolveFixedOffsetFor, offset.Y, Name);
                        return PresenceResult.GiveUp;
                    }
                    break;
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public override void Register(IRegisterContext context)
        {
        }
    }
}
