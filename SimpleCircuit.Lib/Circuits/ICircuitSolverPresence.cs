using SimpleCircuit.Circuits.Contexts;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Represents an item that has a presence in a circuit involving the solver stage. This means it can participate
    /// in the solving of a graphical circuit unknowns.
    /// </summary>
    public interface ICircuitSolverPresence : ICircuitPresence
    {
        /// <summary>
        /// Allows the discovering of aliases. This can reduce the number of unknowns to solve.
        /// </summary>
        /// <param name="context">The context containing the node relationships.</param>
        /// <returns>Returns <c>true</c> if the discovery is successful; or <c>false</c> if the solver should fail.</returns>
        public bool DiscoverNodeRelationships(IRelationshipContext context);

        /// <summary>
        /// Registers the pin's presence in the circuit that will solve all coordinates.
        /// </summary>
        /// <param name="context">The solver context.</param>
        public void Register(IRegisterContext context);

        /// <summary>
        /// Updates the presence with the simulated results for the graphical elements.
        /// </summary>
        /// <param name="context">The update context.</param>
        public void Update(IUpdateContext context);
    }
}
