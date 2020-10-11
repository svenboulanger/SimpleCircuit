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
        /// Finds the difference in angle between a1 and a2 (or a1 - a2).
        /// </summary>
        /// <param name="a1">The first angle.</param>
        /// <param name="a2">The second angle.</param>
        /// <returns>The difference.</returns>
        public static double Difference(double a1, double a2) => Wrap(a1 - a2);

        /// <summary>
        /// Wraps the angle between -pi to pi.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns>The wrapped angle.</returns>
        public static double Wrap(double angle) => angle - Math.Round(angle / Math.PI / 2) * Math.PI * 2;

        /// <summary>
        /// Determines whether the specified value is zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is zero; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZero(this double value) => Math.Abs(value) < 1e-9;

        /// <summary>
        /// Determines whether the two values are equal.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <returns>
        /// <c>true</c> if both values can be considered equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool AreEqual(double a, double b)
        {
            var tol = Math.Max(Math.Abs(a), Math.Abs(b)) * 1e-9;
            return Math.Abs(a - b) < tol;
        }

        /// <summary>
        /// Gets all the component keys and types in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The components in the assembly.</returns>
        public static IEnumerable<ComponentDescription> Components(Assembly assembly)
        {
            foreach (var t in assembly.GetTypes())
            {
                var attributes = t.GetCustomAttributes<SimpleKeyAttribute>(false);
                foreach (var attribute in attributes)
                    yield return new ComponentDescription(attribute.Key, attribute.Name, attribute.Category, t);
            }
        }

        public static void Error(string msg)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = color;
        }
    }
}
