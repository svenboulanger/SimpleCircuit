namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// The different modes of preparation.
    /// </summary>
    public enum PreparationMode
    {
        /// <summary>
        /// The mode of preparation for resetting all presences.
        /// </summary>
        Reset,

        /// <summary>
        /// The mode of preparation for resolving orientation.
        /// </summary>
        Orientation,

        /// <summary>
        /// The mode of preparation for resolving sizes. This can involve measuring/generate text spans.
        /// </summary>
        Sizes,

        /// <summary>
        /// The mode of preparation for resolving (local) offsets. Might be for spacing.
        /// </summary>
        Offsets,

        /// <summary>
        /// The mode of preparation for resolving groups of linked variables.
        /// </summary>
        Groups,

        /// <summary>
        /// The mode of preparation for resolving groups of drawables.
        /// </summary>
        DrawableGroups,
    }
}
