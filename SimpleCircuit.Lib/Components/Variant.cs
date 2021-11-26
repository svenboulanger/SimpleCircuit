using System;
using SimpleCircuit.Components.Variants;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Helper class for creating and managing variant resolvers.
    /// </summary>
    public static class Variant
    {
        /// <summary>
        /// Creates a variant that always resolves.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The variant action.</returns>
        public static VariantAction Do(Action method)
            => new(c => method());

        /// <summary>
        /// Creates a variant that always resolves.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The variant action.</returns>
        public static VariantAction Do(Action<SvgDrawing> method)
            => Do<SvgDrawing>(method);

        /// <summary>
        /// Creates a variant that always resolves.
        /// </summary>
        /// <typeparam name="T">The type argument.</typeparam>
        /// <param name="method">The method.</param>
        /// <returns>The variant action.</returns>
        public static VariantAction Do<T>(Action<T> method)
        {
            return new(context =>
            {
                if (context is IVariantResolverContext<T> c)
                    method(c.Argument);
            });
        }

        /// <summary>
        /// Creates a variant resolver that groups multiple variants.
        /// </summary>
        /// <typeparam name="T">The argument type.</typeparam>
        /// <param name="resolvers">The resolvers.</param>
        /// <returns>The variant selector.</returns>
        public static VariantGroup All(params IVariantResolver[] resolvers)
        {
            var result = new VariantGroup();
            foreach (var item in resolvers)
                result.Children.Add(item);
            return result;
        }

        /// <summary>
        /// Creates a variant resolver that selects the first variant.
        /// </summary>
        /// <param name="resolvers">The resolvers.</param>
        /// <returns>The variant selector.</returns>
        public static VariantSelector FirstOf(params IVariantResolver[] resolvers)
        {
            var result = new VariantSelector();
            foreach (var item in resolvers)
                result.Variants.Add(item);
            return result;
        }

        /// <summary>
        /// Creates a conditional variant resolver that matches the given variants.
        /// </summary>
        /// <typeparam name="T">The argument type.</typeparam>
        /// <param name="include">The variants that need to be included.</param>
        /// <returns>The variant condition builder.</returns>
        public static VariantConditionBuilder If(params string[] include)
            => new(include, null);

        /// <summary>
        /// Creates a conditional variant resolver that does not match the given variants.
        /// </summary>
        /// <typeparam name="T">The argument type.</typeparam>
        /// <param name="exclude">The variants that need to be excluded.</param>
        /// <returns>The variant condition builder.</returns>
        public static VariantConditionBuilder IfNot(params string[] exclude)
            => new(null, exclude);

        /// <summary>
        /// Maps the presence of a variant to a method.
        /// </summary>
        /// <param name="variant">The variant to check.</param>
        /// <param name="method">The method.</param>
        /// <returns>The variant action.</returns>
        public static VariantAction Map(string variant, Action<bool> method)
            => new(context => method(context.Variants.Contains(variant)));

        /// <summary>
        /// Mapes the presence of a variant to a method.
        /// </summary>
        /// <param name="variant">The variant to check.</param>
        /// <param name="method">The method.</param>
        /// <returns>The variant action.</returns>
        public static VariantAction Map(string variant, Action<SvgDrawing, bool> method)
        {
            return new(context =>
            {
                if (context is IVariantResolverContext<SvgDrawing> c)
                    method(c.Argument, context.Variants.Contains(variant));
            });
        }

        /// <summary>
        /// Maps the presence of variants to a method.
        /// </summary>
        /// <param name="variant1">The first variant to check.</param>
        /// <param name="variant2">The second variant to check.</param>
        /// <param name="method">The method.</param>
        /// <returns>The variant map.</returns>
        public static VariantAction Map(string variant1, string variant2, Action<bool, bool> method)
        {
            return new(context => method(
                context.Variants.Contains(variant1),
                context.Variants.Contains(variant2)
                ));
        }

        /// <summary>
        /// Maps the presence of variants to a method.
        /// </summary>
        /// <param name="variant1">The first variant to check.</param>
        /// <param name="variant2">The second variant to check.</param>
        /// <param name="method">The method.</param>
        /// <returns>The variant map.</returns>
        public static VariantAction Map(string variant1, string variant2, Action<SvgDrawing, bool, bool> method)
        {
            return new(context =>
            {
                if (context is IVariantResolverContext<SvgDrawing> c)
                {
                    method(c.Argument,
                        context.Variants.Contains(variant1),
                        context.Variants.Contains(variant2));
                }
            });
        }

        /// <summary>
        /// Maps the presence of variants to a method.
        /// </summary>
        /// <typeparam name="T">The argument type.</typeparam>
        /// <param name="variant1">The first variant to check.</param>
        /// <param name="variant2">The second variant to check.</param>
        /// <param name="variant3">The third variant to check.</param>
        /// <param name="method">The method.</param>
        /// <returns>The variant map.</returns>
        public static VariantAction Map(string variant1, string variant2, string variant3, Action<bool, bool, bool> method)
        {
            return new(context => method(
                context.Variants.Contains(variant1),
                context.Variants.Contains(variant2),
                context.Variants.Contains(variant3)
                ));
        }

        /// <summary>
        /// Maps the presence of variants to a method.
        /// </summary>
        /// <param name="variant1">The first variant to check.</param>
        /// <param name="variant2">The second variant to check.</param>
        /// <param name="variant3">The second variant to check.</param>
        /// <param name="method">The method.</param>
        /// <returns>The variant map.</returns>
        public static VariantAction Map(string variant1, string variant2, string variant3, Action<SvgDrawing, bool, bool, bool> method)
        {
            return new(context =>
            {
                if (context is IVariantResolverContext<SvgDrawing> c)
                {
                    method(c.Argument,
                        context.Variants.Contains(variant1),
                        context.Variants.Contains(variant2),
                        context.Variants.Contains(variant3));
                }
            });
        }
    }
}
