using System.Collections.Generic;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// Represents a variant resolver.
    /// </summary>
    public interface IVariantResolver
    {
        /// <summary>
        /// Collects all potentially used variants.
        /// </summary>
        /// <param name="variants">The variants.</param>
        public void CollectPossibleVariants(ISet<string> variants);

        /// <summary>
        /// Tries to resolve the variant.
        /// </summary>
        /// <param name="variants">The variants.</param>
        /// <returns>Returns <c>true</c> if the variant was used; otherwise, <c>false</c>.</returns>
        public bool Resolve(IVariantResolverContext context);
    }
}
