namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// The different modes of preparation.
    /// </summary>
    public enum PreparationMode
    {
        /// <summary>
        /// The mode of preparation for resolving orientation.
        /// </summary>
        Orientation,

        /// <summary>
        /// The mode of preparation for resolving (local) offsets. Might be for spacing.
        /// </summary>
        Offsets,

        /// <summary>
        /// The mode of preparation for resolving variable groups.
        /// </summary>
        Groups,
    }
}
