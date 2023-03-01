using SpiceSharp.Entities;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// Describes a context for adding unknowns for solving.
    /// </summary>
    public interface IRegisterContext
    {
        /// <summary>
        /// Gets the circuit that will be solved.
        /// </summary>
        public IEntityCollection Circuit { get; }

        /// <summary>
        /// Gets the context of the relationships between nodes that will belong to the circuit.
        /// </summary>
        public IRelationshipContext Relationships { get; }

        /// <summary>
        /// Gets or sets whether the circuit should be recalculated.
        /// </summary>
        public bool Recalculate { get; set; }
    }
}
