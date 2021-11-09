using System;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Bounds.
    /// </summary>
    public struct Bounds : IEquatable<Bounds>, IFormattable
    {
        /// <summary>
        /// Gets the left.
        /// </summary>
        public double Left { get; }

        /// <summary>
        /// Gets the right.
        /// </summary>
        public double Right { get; }

        /// <summary>
        /// Gets the top.
        /// </summary>
        public double Top { get; }

        /// <summary>
        /// Gets the bottom.
        /// </summary>
        public double Bottom { get; }

        /// <summary>
        /// Gets the width of the bounds.
        /// </summary>
        public double Width => Right - Left;

        /// <summary>
        /// Gets the height of the bounds.
        /// </summary>
        public double Height => Bottom - Top;

        /// <summary>
        /// Creates new bounds.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="top">The top argument.</param>
        /// <param name="right">The right argument.</param>
        /// <param name="bottom">The bottom argument.</param>
        public Bounds(double left, double top, double right, double bottom)
        {
            if (left < right)
            {
                Left = left;
                Right = right;
            }
            else
            {
                Right = left;
                Left = right;
            }
            if (top < bottom)
            {
                Top = top;
                Bottom = bottom;
            }
            else
            {
                Top = bottom;
                Bottom = top;
            }
        }

        /// <summary>
        /// Checks equality with other bound.
        /// </summary>
        /// <param name="other">The other bounds.</param>
        /// <returns>Returns <c>true</c> if the bounds are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(Bounds other)
        {
            if (!Left.Equals(other.Left))
                return false;
            if (!Right.Equals(other.Right))
                return false;
            if (!Top.Equals(other.Top))
                return false;
            if (!Bottom.Equals(other.Bottom))
                return false;
            return true;
        }

        public override string ToString()
        {
            return $"({Left}; {Top}; {Right}; {Bottom})";
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return "(" +
                Left.ToString(format, formatProvider) + "; " +
                Top.ToString(format, formatProvider) + "; " +
                Right.ToString(format, formatProvider) + "; " +
                Bottom.ToString(format, formatProvider) + ")";
        }
    }
}
