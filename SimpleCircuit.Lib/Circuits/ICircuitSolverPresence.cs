using SimpleCircuit.Diagnostics;
using SpiceSharp.Simulations;

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
        /// <param name="diagnostics">The diagnostics handler.</param>
        public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics);

        /// <summary>
        /// Registers the pin's presence in the circuit that will solve all coordinates.
        /// </summary>
        /// <param name="context">The context for simulation the graphical elements of the circuit.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        public void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics);

        /// <summary>
        /// Updates the presence with the simulated results for the graphical elements.
        /// </summary>
        /// <param name="state">The state containing the simulation results.</param>
        /// <param name="context">The context previously used to build the simulation.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        public void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics);
    }
}
