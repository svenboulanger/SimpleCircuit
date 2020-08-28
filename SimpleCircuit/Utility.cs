using SimpleCircuit.Contributions;
using System;
using System.Runtime.CompilerServices;

namespace SimpleCircuit
{
    public static class Utility
    {
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

        public static ConstantContributor X(this double value) => new ConstantContributor(UnknownTypes.X, value);
        public static ConstantContributor Y(this double value) => new ConstantContributor(UnknownTypes.Y, value);
        public static ConstantContributor A(this double value) => new ConstantContributor(UnknownTypes.Angle, value);
        public static ConstantContributor SX(this double value) => new ConstantContributor(UnknownTypes.ScaleX, value);
        public static ConstantContributor SY(this double value) => new ConstantContributor(UnknownTypes.ScaleY, value);
        public static ConstantContributor Scalar(this double value) => new ConstantContributor(UnknownTypes.Scalar, value);
        public static IContributor Add(this IContributor b, double value) => new OffsetContributor(b, value, b.Type);
        public static IContributor Subtract(this IContributor b, double value) => new OffsetContributor(b, -value, b.Type);
    }
}
