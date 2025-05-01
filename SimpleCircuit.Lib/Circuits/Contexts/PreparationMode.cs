namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// The different modes of preparation.
    /// </summary>
    public enum PreparationMode
    {
        /// <summary>
        /// The mode of preparation for resetting all presences.
        /// This mode can be used to build up everything based on variants and properties.
        /// </summary>
        Reset,

        /// <summary>
        /// The mode of preparation for finding links and references.
        /// This mode can be used to ask information from other presences. Such as finding pins.
        /// </summary>
        Find,

        /// <summary>
        /// The mode of preparation for resolving orientation of presences and pins.
        /// </summary>
        Orientation,

        /// <summary>
        /// The mode of preparation for resolving sizes of presences.
        /// This can involve measuring/generating text spans.
        /// </summary>
        Sizes,

        /// <summary>
        /// The mode of preparation for resolving (local) fixed offsets between variables.
        /// Might be used for spacing.
        /// </summary>
        Offsets,

        /// <summary>
        /// The mode of preparation for resolving groups of linked variables.
        /// Grouped variables will be tied together as one coherent visual group.
        /// </summary>
        Groups,

        /// <summary>
        /// The mode of preparation for tying drawables to groups.
        /// Any presence that will take up some area of the coherent visual group can choose to register.
        /// </summary>
        DrawableGroups,
    }
}
