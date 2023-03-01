using SimpleCircuit.Diagnostics;
using SpiceSharp.Entities;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// Implementation of a <see cref="IRegisterContext"/>.
    /// </summary>
    public class RegisterContext : IRegisterContext
    {
        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; }

        /// <inheritdoc />
        public IEntityCollection Circuit { get; }

        /// <inheritdoc />
        public IRelationshipContext Relationships { get; }

        /// <inheritdoc />
        public bool Recalculate { get; set; }

        /// <summary>
        /// Creates a new context for simulation of graphical items.
        /// </summary>
        /// <param name="circuit">The circuit elements for simulation.</param>
        /// <param name="relationshipContext">Extra data for the nodes.</param>
        public RegisterContext(IDiagnosticHandler diagnostics, IRelationshipContext relationshipContext, IEntityCollection circuit = null)
        {
            Diagnostics = diagnostics;
            Circuit = circuit ?? new SpiceSharp.Circuit();
            Relationships = relationshipContext;
        }
    }
}
