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
        /// <param name="diagnostics">The diagnostics.</param>
        public void Prepare(GraphicalCircuit circuit, IDiagnosticHandler diagnostics);
    }
}
