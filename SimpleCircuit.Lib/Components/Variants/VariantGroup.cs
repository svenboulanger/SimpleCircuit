using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// A variant that runs all variants in the group.
    /// </summary>
    public class VariantGroup : IVariantResolver
    {
        /// <summary>
        /// Gets the variants that will all be processed.
        /// </summary>
        public List<IVariantResolver> Children { get; } = new();

        /// <inheritdoc />
        public void CollectPossibleVariants(ISet<string> variants)
        {
            foreach (var child in Children)
                child.CollectPossibleVariants(variants);
        }

        /// <summary>
        /// Includes a variant in the group.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="variant"/> is <c>null</c>.</exception>
        public VariantGroup With(IVariantResolver variant)
        {
            Children.Add(variant ?? throw new ArgumentNullException(nameof(variant)));
            return this;
        }

        /// <inheritdoc />
        public bool Resolve(IVariantResolverContext context)
        {
            bool result = false;
            foreach (var variant in Children)
                result |= variant.Resolve(context);
            return result;
        }
    }
}
