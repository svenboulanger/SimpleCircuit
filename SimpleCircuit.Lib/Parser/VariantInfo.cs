namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Represents variant information.
    /// </summary>
    public struct VariantInfo
    {
        /// <summary>
        /// Determines whether the variant should be included. If <c>false</c>, the
        /// variant should be removed.
        /// </summary>
        public bool Include { get; }

        /// <summary>
        /// The name of the variant.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new <see cref="VariantInfo"/>.
        /// </summary>
        /// <param name="include">If <c>true</c>, the variant should be added; otherwise, the variant should be removed.</param>
        /// <param name="name">The variant name.</param>
        public VariantInfo(bool include, string name)
        {
            Include = include;
            Name = name;
        }
    }
}
