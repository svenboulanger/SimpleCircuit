using SimpleCircuit.Circuits.Contexts;

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
        /// <param name="diagnostics">The diagnostics handler.</param>
        public bool Reset(IResetContext diagnostics);

        /// <summary>
        /// Prepares the circuit presence for resolving a graphical circuit.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="context">The preparation context.</param>
        /// <returns>
        ///     Returns the result of the preparation.
        /// </returns>
        public PresenceResult Prepare(IPrepareContext context);
    }
}
