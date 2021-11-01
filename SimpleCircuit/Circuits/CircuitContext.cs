using SpiceSharp.Entities;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A context for simulation of graphical items.
    /// </summary>
    public class CircuitContext
    {
        /// <summary>
        /// Gets the circuit that will be solved.
        /// </summary>
        public IEntityCollection Circuit { get; }

        /// <summary>
        /// Gets the context of the nodes that will belong to the circuit.
        /// </summary>
        public NodeContext Nodes { get; }

        /// <summary>
        /// Creates a new context for simulation of graphical items.
        /// </summary>
        /// <param name="circuit">The circuit elements for simulation.</param>
        /// <param name="nodes">Extra data for the nodes.</param>
        public CircuitContext(IEntityCollection circuit = null, NodeContext nodes = null)
        {
            Circuit = circuit ?? new SpiceSharp.Circuit();
            Nodes = nodes ?? new NodeContext();
        }
    }
}
