namespace SimpleCircuit.Components
{
    /// <summary>
    /// An circuit presence that has coordinates in 2D space.
    /// </summary>
    public interface ILocatedPresence : ICircuitPresence
    {
        /// <summary>
        /// Gets the name of the node that contains the X-location.
        /// </summary>
        public string X { get; }

        /// <summary>
        /// Gets the name of the node that contains the Y-location.
        /// </summary>
        public string Y { get; }

        /// <summary>
        /// Gets the location of the item.
        /// </summary>
        public Vector2 Location { get; }
    }
}
