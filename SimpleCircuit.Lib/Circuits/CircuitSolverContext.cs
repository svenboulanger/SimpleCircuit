using SimpleCircuit.Components.Wires;
using SpiceSharp.Entities;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A context for simulation of graphical items.
    /// </summary>
    public class CircuitSolverContext
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
        /// Gets or sets whether the circuit should be recalculated.
        /// </summary>
        public bool Recalculate { get; set; }

        /// <summary>
        /// The wire segments defined until now.
        /// </summary>
        public List<WireSegment> WireSegments { get; } = new List<WireSegment>();

        /// <summary>
        /// Creates a new context for simulation of graphical items.
        /// </summary>
        /// <param name="circuit">The circuit elements for simulation.</param>
        /// <param name="nodes">Extra data for the nodes.</param>
        public CircuitSolverContext(IEntityCollection circuit = null, NodeContext nodes = null)
        {
            Circuit = circuit ?? new SpiceSharp.Circuit();
            Nodes = nodes ?? new NodeContext();
        }
    }
}
