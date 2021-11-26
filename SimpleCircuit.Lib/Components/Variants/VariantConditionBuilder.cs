using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// A class for building conditional variants.
    /// </summary>
    public class VariantConditionBuilder
    {
        private readonly IEnumerable<string> _includes, _excludes;

        /// <summary>
        /// Creates a new variant condition builder.
        /// </summary>
        public VariantConditionBuilder(IEnumerable<string> includes, IEnumerable<string> excludes)
        {
            _includes = includes;
            _excludes = excludes;
        }

        public VariantConditionBuilder And(params string[] include)
            => new(_includes == null ? include : _includes.Union(include), _excludes);
        public VariantConditionBuilder AndNot(params string[] exclude)
            => new(_includes, _excludes == null ? exclude : _excludes.Union(exclude));

        public VariantCondition Do(IVariantResolver resolver)
            => new(_includes, _excludes, resolver, null);
        public VariantCondition DoElse(IVariantResolver ifTrue, IVariantResolver ifFalse)
            => new(_includes, _excludes, ifTrue, ifFalse);

        public VariantCondition Do(Action action) => Do(Variant.Do(action));
        public VariantCondition Do(Action<SvgDrawing> action) => Do(Variant.Do(action));
        public VariantCondition Do<T>(Action<T> action) => Do(Variant.Do(action));

        public VariantCondition DoElse(Action ifTrue, Action ifFalse) => DoElse(Variant.Do(ifTrue), Variant.Do(ifFalse));
        public VariantCondition DoElse(Action<SvgDrawing> ifTrue, Action<SvgDrawing> ifFalse) => DoElse(Variant.Do(ifTrue), Variant.Do(ifFalse));
        public VariantCondition DoElse<T>(Action<T> ifTrue, Action<T> ifFalse) => DoElse(Variant.Do(ifTrue), Variant.Do(ifFalse));
    }
}
