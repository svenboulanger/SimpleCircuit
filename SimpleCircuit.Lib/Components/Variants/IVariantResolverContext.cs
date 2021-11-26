using System.Collections.Generic;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// A context for resolving variants.
    /// </summary>
    public interface IVariantResolverContext
    {
        /// <summary>
        /// Gets the defined variants.
        /// </summary>
        public ISet<string> Variants { get; }
    }

    /// <summary>
    /// A context for resolving variants with an additional argument.
    /// </summary>
    /// <typeparam name="T">The type argument.</typeparam>
    public interface IVariantResolverContext<T>
    {
        /// <summary>
        /// Gets the argument.
        /// </summary>
        public T Argument { get; }
    }
}
