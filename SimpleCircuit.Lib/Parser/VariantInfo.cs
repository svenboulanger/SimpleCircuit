namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Represents variant information.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="VariantInfo"/>.
    /// </remarks>
    /// <param name="include">If <c>true</c>, the variant should be added; otherwise, the variant should be removed.</param>
    /// <param name="name">The variant name.</param>
    public readonly struct VariantInfo(bool include, string name)
    {
        /// <summary>
        /// Determines whether the variant should be included. If <c>false</c>, the
        /// variant should be removed.
        /// </summary>
        public bool Include { get; } = include;

        /// <summary>
        /// The name of the variant.
        /// </summary>
        public string Name { get; } = name;
    }
}
