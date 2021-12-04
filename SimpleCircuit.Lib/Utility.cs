using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleCircuit
{
    /// <summary>
    /// General helper methods.
    /// </summary>
    public static class Utility
    {
        public class ComponentDescription
        {
            public string Key { get; }
            public string Name { get; }
            public string Category { get; }
            public Type Type { get; }
            public ComponentDescription(string key, string name, string category, Type type)
            {
                Key = key;
                Name = name;
                Category = category;
                Type = type;
            }
        }

        /// <summary>
        /// Determines whether the specified value is zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is zero; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZero(this double value) => Math.Abs(value) < 1e-9;

        /// <summary>
        /// Gets all the component keys and types in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The components in the assembly.</returns>
        public static IEnumerable<ComponentDescription> Components(Assembly assembly)
        {
            yield break;
        }
    }
}
