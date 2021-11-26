using System;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// A variant action.
    /// </summary>
    public class VariantAction : IVariantResolver
    {
        private readonly Action<IVariantResolverContext> _action;

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
        public bool Resolve(IVariantResolverContext context)
        {
            _action(context);
            return true;
        }
    }
}
