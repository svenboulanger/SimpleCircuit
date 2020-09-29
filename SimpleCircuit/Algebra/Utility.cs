using System;
using System.Globalization;

namespace SimpleCircuit.Algebra
{
    /// <summary>
    /// Some utility methods
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Gets or sets the separator used when combining strings.
        /// </summary>
        public static string Separator { get; set; } = "/";

        /// <summary>
        /// Format a string using the current culture.
        /// </summary>
        /// <param name="format">The formatting.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>
        /// The formatted string.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="format"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">
        /// Thrown if <paramref name="format"/> is invalid, or if the index of a format item is not higher than 0.
        /// </exception>
        public static string FormatString(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Combines a name with the specified appendix, using <see cref="Separator" />.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="appendix">The appendix.</param>
        /// <returns>
        /// The combined string.
        /// </returns>
        public static string Combine(this string name, string appendix)
        {
            if (name == null)
                return appendix;
            return name + Separator + appendix;
        }

        /// <summary>
        /// Throws an exception if the object is null.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="source">The object.</param>
        /// <param name="name">The parameter name.</param>
        /// <returns>
        /// The original object.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static T ThrowIfNull<T>(this T source, string name)
        {
            if (source == null)
                throw new ArgumentNullException(name);
            return source;
        }

        /// <summary>
        /// Throws an exception if the value is not greater than the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than <paramref name="limit"/>.</exception>
        public static double GreaterThan(this double value, string name, double limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, "Invalid property");
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not less than the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not less than <paramref name="limit"/>.</exception>
        public static double LessThan(this double value, string name, double limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, "Invalid property");
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not greater than or equal to the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than or equal to <paramref name="limit"/>.</exception>
        public static double GreaterThanOrEquals(this double value, string name, double limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, "Invalid property");
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not less than or equal to the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not less than or equal to the specified limit.</exception>
        public static double LessThanOrEquals(this double value, string name, double limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, "Invalid property");
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not greater than the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than <paramref name="limit"/>.</exception>
        public static int GreaterThan(this int value, string name, int limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, "Invalid property");
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not less than the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not less than <paramref name="limit"/>.</exception>
        public static int LessThan(this int value, string name, int limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, "Invalid property");
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not greater than or equal to the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than or equal to <paramref name="limit"/>.</exception>
        public static int GreaterThanOrEquals(this int value, string name, int limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, "Invalid property");
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not less than or equal to the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not less than or equal to the specified limit.</exception>
        public static int LessThanOrEquals(this int value, string name, int limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, "Invalid property");
            return value;
        }

        public static void Print(this ISparseSolver<double> solver)
        {
            var text = new string[solver.Size][];
            var widths = new int[solver.Size + 1];
            for (var i = 0; i < solver.Size; i++)
            {
                text[i] = new string[solver.Size + 1];
                Element<double> elt;
                for (var j = 0; j < solver.Size; j++)
                {
                    elt = solver.FindElement(new MatrixLocation(i + 1, j + 1));
                    text[i][j] = elt == null ? "." : $"{elt.Value:G3}";
                    widths[j] = Math.Max(widths[j], text[i][j].Length);
                }
                elt = solver.FindElement(i + 1);
                text[i][solver.Size] = elt == null ? "." : $"{elt.Value:G3}";
                widths[solver.Size] = Math.Max(widths[solver.Size], text[i][solver.Size].Length);
            }

            // Write the string
            for (var i = 0; i < solver.Size; i++)
            {
                for (var j = 0; j < solver.Size; j++)
                {
                    Console.Write(new string(' ', widths[j] - text[i][j].Length));
                    Console.Write(text[i][j]);
                    Console.Write(" ");
                }
                Console.Write("| ");
                Console.WriteLine(text[i][solver.Size]);
            }
        }

        public static void PrintReordered(this ISparsePivotingSolver<double> solver)
        {
            solver.Precondition((matrix, rhs) =>
            {
                var text = new string[solver.Size][];
                var widths = new int[solver.Size + 1];
                for (var i = 0; i < solver.Size; i++)
                {
                    text[i] = new string[solver.Size + 1];
                    Element<double> elt;
                    for (var j = 0; j < solver.Size; j++)
                    {
                        elt = matrix.FindElement(new MatrixLocation(i + 1, j + 1));
                        text[i][j] = elt == null ? "." : $"{elt.Value:G3}";
                        widths[j] = Math.Max(widths[j], text[i][j].Length);
                    }
                    elt = rhs.FindElement(i + 1);
                    text[i][solver.Size] = elt == null ? "." : $"{elt.Value:G3}";
                    widths[solver.Size] = Math.Max(widths[solver.Size], text[i][solver.Size].Length);
                }

                // Write the string
                for (var i = 0; i < solver.Size; i++)
                {
                    for (var j = 0; j < solver.Size; j++)
                    {
                        Console.Write(new string(' ', widths[j] - text[i][j].Length));
                        Console.Write(text[i][j]);
                        Console.Write(" ");
                    }
                    Console.Write("| ");
                    Console.WriteLine(text[i][solver.Size]);
                }
            });
        }
    }
}
