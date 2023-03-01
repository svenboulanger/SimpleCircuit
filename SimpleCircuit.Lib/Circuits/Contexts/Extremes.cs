namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// A struct that tracks the bounds of nodes using relative specifications.
    /// </summary>
    public readonly struct Extremes
    {
        /// <summary>
        /// Tracks linked nodes.
        /// </summary>
        public NodeGrouper Linked { get; } = new NodeGrouper();

        /// <summary>
        /// Tracks the minima in X-coordinates.
        /// </summary>
        public NodeExtremeFinder Minimum { get; } = new NodeExtremeFinder();

        /// <summary>
        /// Tracks the maxima in X-coordinates.
        /// </summary>
        public NodeExtremeFinder Maximum { get; } = new NodeExtremeFinder();

        /// <summary>
        /// Enforces an order on coordinates.
        /// </summary>
        /// <param name="smallest">The smallest coordinate.</param>
        /// <param name="largest">The largest coordinate.</param>
        public void Order(string smallest, string largest)
        {
            if (largest.Equals(smallest))
                return; // We can't order same nodes...
            Minimum.Order(smallest, largest);
            Maximum.Order(largest, smallest);
            Linked.Group(smallest, largest);
        }

        /// <summary>
        /// Creates a new <see cref="Extremes"/> struct.
        /// </summary>
        public Extremes()
        {
        }
    }
}
