using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A loose pin, i.e. a pin that is not related to anything else.
    /// The owner is responsible for not letting this pin escape!
    /// </summary>
    public class LoosePin : Pin, IOrientedPin
    {
        /// <summary>
        /// Creates a loose pin. This means that any constrains need to be applied manually!
        /// </summary>
        /// <param name="name">The name of the pin.</param>
        /// <param name="description">The pin description.</param>
        /// <param name="owner">The owner of the pin.</param>
        public LoosePin(string name, string description, ILocatedDrawable owner)
            : base(name, description, owner)
        {
        }

        /// <inheritdoc />
        public Vector2 Orientation { get; private set; }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            // Left to whoever owns this pin...
        }

        /// <inheritdoc />
        public override void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            // Left to whoever owns this pin...
        }

        /// <inheritdoc />
        public bool ResolveOrientation(Vector2 orientation, IDiagnosticHandler diagnostics)
        {
            // We are not being difficult, just give the orientation it wants...
            Orientation = orientation;
            return true;
        }
    }
}
