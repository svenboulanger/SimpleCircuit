namespace SimpleCircuit.Components
{
    /// <summary>
    /// Enumeration of possible modes for discovering node relationships.
    /// </summary>
    public enum NodeRelationMode
    {
        /// <summary>
        /// No mode.
        /// </summary>
        None,

        /// <summary>
        /// Find fixed relationships between nodes.
        /// </summary>
        Offsets,

        /// <summary>
        /// Find groups of nodes that should be solved together.
        /// </summary>
        Groups,
    }
}
