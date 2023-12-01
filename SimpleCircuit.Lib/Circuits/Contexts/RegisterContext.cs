using SimpleCircuit.Diagnostics;
using SpiceSharp.Entities;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// Implementation of a <see cref="IRegisterContext"/>.
    /// </summary>
    /// <remarks>
    /// Creates a new context for simulation of graphical items.
    /// </remarks>
    /// <param name="circuit">The circuit elements for simulation.</param>
    /// <param name="relationshipContext">Extra data for the nodes.</param>
    public class RegisterContext(IDiagnosticHandler diagnostics, IRelationshipContext relationshipContext, IEntityCollection circuit = null) : IRegisterContext
    {
        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; } = diagnostics;

        /// <inheritdoc />
        public IEntityCollection Circuit { get; } = circuit ?? new SpiceSharp.Circuit();

        /// <inheritdoc />
        public IRelationshipContext Relationships { get; } = relationshipContext;

        /// <inheritdoc />
        public bool Recalculate { get; set; }
    }
}
