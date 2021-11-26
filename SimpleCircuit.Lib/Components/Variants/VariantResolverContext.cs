using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// A context for resolving variants.
    /// </summary>
    public class VariantResolverContext : IVariantResolverContext
    {
        /// <inheritdoc />
        public ISet<string> Variants { get; }

        public VariantResolverContext(ISet<string> variants)
        {
            Variants = variants ?? throw new ArgumentNullException(nameof(variants));
        }
    }

    /// <summary>
    /// A context for resolving variants.
    /// </summary>
    /// <typeparam name="T">The type argument.</typeparam>
    public class VariantResolverContext<T> : VariantResolverContext, IVariantResolverContext<T>
    {
        /// <inheritdoc />
        public T Argument { get; }

        public VariantResolverContext(ISet<string> variants, T argument)
            : base(variants)
        {
            Argument = argument;
        }
    }
}
