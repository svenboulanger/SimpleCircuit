using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Parser;
using System.Collections.Generic;

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
        /// Gets a list that can be used to store where the presence is referenced.
        /// </summary>
        public List<TextLocation> Sources { get; }

        /// <summary>
        /// Gets the order in which the presence needs to be executed.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Prepares the circuit presence for resolving a graphical circuit.
        /// </summary>
        /// <param name="context">The preparation context.</param>
        /// <returns>
        ///     Returns the result of the preparation.
        /// </returns>
        public PresenceResult Prepare(IPrepareContext context);
    }
}
