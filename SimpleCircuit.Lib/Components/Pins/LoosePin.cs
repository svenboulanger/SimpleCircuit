using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A loose pin, i.e. a pin that is not related to anything else.
    /// The owner is responsible for not letting this pin escape!
    /// </summary>
    /// <remarks>
    /// Creates a loose pin. This means that any constrains need to be applied manually!
    /// </remarks>
    /// <param name="name">The name of the pin.</param>
    /// <param name="description">The pin description.</param>
    /// <param name="owner">The owner of the pin.</param>
    public class LoosePin(string name, string description, ILocatedDrawable owner) : Pin(name, description, owner), IOrientedPin
    {
        /// <inheritdoc />
        public bool HasFixedOrientation { get; private set; }

        /// <inheritdoc />
        public Vector2 Orientation { get; private set; }

        /// <inheritdoc />
        public bool HasFreeOrientation => !HasFixedOrientation;

        /// <inheritdoc />
        public override PresenceResult Prepare(IPrepareContext context)
        {
            var result = base.Prepare(context);
            if (result == PresenceResult.GiveUp)
                return result;

            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    HasFixedOrientation = false;
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        public override void Register(IRegisterContext context)
        {
            // Left to whoever owns this pin...
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
    }
}
