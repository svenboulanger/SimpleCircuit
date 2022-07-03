using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Represents a presence in a graphical circuit.
    /// </summary>
    public interface ICircuitPresence
    {
        /// <summary>
        /// Gets the name of the presence.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the order in which the presence needs to be executed.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Resets the circuit presence before resolving a graphical circuit.
        /// </summary>
        public void Reset();

        /// <summary>
        /// Prepares the circuit presence for resolving a graphical circuit.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="mode">The current mode of operation.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <returns>
        ///     Returns <c>true</c> if the presence successfully prepared; otherwise, <c>false</c>,
        ///     in which case the graphical circuit should prepare this circuit presence again after
        ///     first preparing all other presences.
        /// </returns>
        public PresenceResult Prepare(GraphicalCircuit circuit, PresenceMode mode, IDiagnosticHandler diagnostics);
    }
}
