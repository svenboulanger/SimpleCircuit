namespace SimpleCircuit.Components
{
    /// <summary>
    /// The context used for collecting information about nodes in a
    /// graphical circuit.
    /// </summary>
    public class NodeContext
    {
        /// <summary>
        /// Gets the graphical circuit defining the nodes.
        /// </summary>
        public GraphicalCircuit Circuit { get; }

        /// <summary>
        /// Gets the groups of nodes that are shorted together, i.e. are exactly the same.
        /// </summary>
        public NodeGrouper Shorts { get; } = new();

        /// <summary>
        /// Get the groups of nodes that are constrained relative to each other.
        /// </summary>
        public NodeGrouper Relative { get; } = new();
    }
}
