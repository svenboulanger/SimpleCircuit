using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// A variant resolver that only continues if the variants match the condition.
    /// </summary>
    public class VariantCondition : IVariantResolver
    {
        private readonly HashSet<string> _include, _exclude;
        private readonly IVariantResolver _ifTrue, _ifFalse;

        /// <summary>
        /// Creates a new conditional variant.
        /// </summary>
        public VariantCondition(IEnumerable<string> include, IEnumerable<string> exclude, IVariantResolver ifTrue, IVariantResolver ifFalse)
        {
            if (include != null)
            {
                foreach (var incl in include)
                {
                    if (_include == null)
                        _include = new(StringComparer.OrdinalIgnoreCase);
                    _include.Add(incl);
                }
            }
            if (exclude != null)
            {
                foreach (var excl in exclude)
                {
                    if (_exclude == null)
                        _exclude = new(StringComparer.OrdinalIgnoreCase);
                    _exclude.Add(excl);
                }
            }
            _ifTrue = ifTrue;
            _ifFalse = ifFalse;
        }

        /// <inheritdoc />
        public bool Resolve(IVariantResolverContext context)
        {
            if (_include != null && !_include.IsSubsetOf(context.Variants))
                return _ifFalse?.Resolve(context) ?? false;
            if (_exclude != null && _exclude.Overlaps(context.Variants))
                return _ifFalse?.Resolve(context) ?? false;
            return _ifTrue?.Resolve(context) ?? false;
        }
    }
}
