namespace SimpleCircuit.Parser.Variants
{
    /// <summary>
    /// Describes a context with variants.
    /// </summary>
    public interface IVariantContext
    {
        /// <summary>
        /// Checks whether the variant is defined.
        /// </summary>
        /// <param name="variant">The variant name.</param>
        /// <returns>Returns <c>true</c> if the variant exists; otherwise, <c>false</c>.</returns>
        bool Contains(string variant);
    }
}
