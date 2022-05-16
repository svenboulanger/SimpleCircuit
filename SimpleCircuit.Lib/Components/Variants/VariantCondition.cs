using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// A variant resolver that only continues if the variants match the condition.
    /// </summary>
    public class VariantCondition : IVariantResolver
    {
        private HashSet<string> _include, _exclude;
        private IVariantResolver _ifTrue, _ifFalse;

        /// <summary>
        /// Makes the variant required for the condition.
        /// </summary>
        /// <param name="names">The name of the variant.</param>
        public void If(params string[] names)
        {
            if (names == null || names.Length == 0)
                return;
            if (_include == null)
                _include = new(StringComparer.OrdinalIgnoreCase);
            foreach (string name in names)
                _include.Add(name);
        }

        /// <summary>
        /// Avoids the variant in the condition.
        /// </summary>
        /// <param name="names">The name of the variant.</param>
        public void IfNot(params string[] names)
        {
            if (names == null || names.Length == 0)
                return;
            if (_exclude == null)
                _exclude = new(StringComparer.OrdinalIgnoreCase);
            foreach (string name in names)
                _exclude.Add(name);
        }

        /// <summary>
        /// Adds a variant resolver to be run when the condition evaluates to true.
        /// </summary>
        /// <param name="ifTrue">The resolver run when all variants are there.</param>
        public VariantCondition Then(IVariantResolver ifTrue)
        {
            if (_ifTrue == null)
                _ifTrue = ifTrue;
            else
            {
                if (_ifTrue is VariantGroup group)
                    group.With(ifTrue);
                else
                {
                    group = new VariantGroup();
                    group.With(ifTrue);
                    _ifTrue = group;
                }
            }
            return this;
        }

        /// <summary>
        /// Adds a variant resolver to be run when the condition evaluates to true.
        /// </summary>
        /// <param name="ifTrue">The resolver run when all variants are there.</param>
        public VariantCondition Then(Action<SvgDrawing> ifTrue) => Then(Variant.Do(ifTrue));

        /// <summary>
        /// Adds a variant resolver to be run when the condition evaluates to false.
        /// </summary>
        /// <param name="ifFalse">The resolver run when any of the variants is not there.</param>
        public VariantCondition Else(IVariantResolver ifFalse)
        {
            if (_ifFalse == null)
                _ifFalse = ifFalse;
            else
            {
                if (_ifFalse is VariantGroup group)
                    group.With(ifFalse);
                else
                {
                    group = new VariantGroup();
                    group.With(ifFalse);
                    _ifFalse = group;
                }
            }
            return this;
        }

        /// <summary>
        /// Adds a variant resolver to be run when the condition evaluates to false.
        /// </summary>
        /// <param name="ifFalse">The resolver run when any of the variants is not there.</param>
        public VariantCondition Else(Action<SvgDrawing> ifFalse) => Else(Variant.Do(ifFalse));

        /// <inheritdoc />
        public void CollectPossibleVariants(ISet<string> variants)
        {
            if (_include != null)
            {
                foreach (string variant in _include)
                    variants.Add(variant);
            }
            if (_exclude != null)
            {
                foreach (string variant in _exclude)
                    variants.Add(variant);
            }
            _ifTrue?.CollectPossibleVariants(variants);
            _ifFalse?.CollectPossibleVariants(variants);
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
