using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// A variant action.
    /// </summary>
    public class VariantAction : IVariantResolver
    {
        private readonly Action<IVariantResolverContext> _action;

        public HashSet<string> Dependencies { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates a new variant.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <c>null</c>.</exception>
        public VariantAction(Action<IVariantResolverContext> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <inheritdoc />
        public void CollectPossibleVariants(ISet<string> variants)
        {
            foreach (string name in Dependencies)
                variants.Add(name);
        }

        /// <inheritdoc />
        public bool Resolve(IVariantResolverContext context)
        {
            _action(context);
            return true;
        }
    }
}
