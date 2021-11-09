namespace SimpleCircuit.Components
{
    /// <summary>
    /// The context used for collecting information about nodes in a
    /// graphical circuit.
    /// </summary>
    public class NodeContext
    {
        /// <summary>
        /// Gets the node groups of nodes that are shorted together.
        /// </summary>
        public NodeGrouper Shorts { get; } = new();
    }
}
