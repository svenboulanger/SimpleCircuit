namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// Represents a variant resolver.
    /// </summary>
    public interface IVariantResolver
    {
        /// <summary>
        /// Tries to resolve the variant.
        /// </summary>
        /// <param name="variants">The variants.</param>
        /// <returns>Returns <c>true</c> if the variant was used; otherwise, <c>false</c>.</returns>
        public bool Resolve(IVariantResolverContext context);
    }
}
