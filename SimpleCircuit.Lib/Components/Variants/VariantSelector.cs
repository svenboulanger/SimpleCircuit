using System.Collections.Generic;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// A variant that only selects the first one that matches.
    /// </summary>
    public class VariantSelector : IVariantResolver
    {
        /// <summary>
        /// Gets the variants that need to be selected.
        /// </summary>
        public List<IVariantResolver> Children { get; } = new();

        /// <inheritdoc />
        public void CollectPossibleVariants(ISet<string> variants)
        {
            foreach (var variant in Children)
                variant.CollectPossibleVariants(variants);
        }

        /// <inheritdoc />
        public bool Resolve(IVariantResolverContext context)
        {
            foreach (var variant in Children)
            {
                if (variant.Resolve(context))
                    return true;
            }
            return false;
        }
    }
}
