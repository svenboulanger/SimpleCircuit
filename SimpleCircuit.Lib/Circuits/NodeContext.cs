using SimpleCircuit.Circuits;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{

    /// <summary>
    /// The context used for collecting information about nodes in a
    /// graphical circuit.
    /// </summary>
    public class NodeContext
    {
        /// <summary>
        /// Gets the current mode for discovering node relationships.
        /// </summary>
        public NodeRelationMode Mode { get; set; }

        /// <summary>
        /// Gets the graphical circuit defining the nodes.
        /// </summary>
        public GraphicalCircuit Circuit { get; }

        /// <summary>
        /// Gets the groups of nodes that are shorted together, i.e. are exactly the same.
        /// </summary>
        public NodeGrouper Shorts { get; } = new();

        /// <summary>
        /// Gets the extremes of the nodes.
        /// </summary>
        public Extremes Extremes { get; } = new();

        /// <summary>
        /// Gets a set of X- and Y-coordinate node combinations.
        /// </summary>
        public HashSet<XYNode> XYSets { get; } = new();
    }
}
