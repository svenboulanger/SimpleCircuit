using System;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Bounds.
    /// </summary>
    public readonly struct Bounds : IEquatable<Bounds>, IFormattable
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
        /// Creates new bounds.
        /// </summary>
        /// <param name="p1">One of the corners of the bounds.</param>
        /// <param name="p2">The other corner of the bounds.</param>
        public Bounds(Vector2 p1, Vector2 p2)
        {
            if (p1.X < p2.X)
            {
                Left = p1.X;
                Right = p2.X;
            }
            else
            {
                Left = p2.X;
                Right = p1.X;
            }
            if (p1.Y < p2.Y)
            {
                Top = p1.Y;
                Bottom = p2.Y;
            }
            else
            {
                Top = p2.Y;
                Bottom = p1.Y;
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

        /// <summary>
        /// Overloads addition for bounds.
        /// </summary>
        public static Bounds operator +(Vector2 offset, Bounds bounds)
            => new(offset.X + bounds.Left, offset.Y + bounds.Top, offset.X + bounds.Right, offset.Y + bounds.Bottom);

        /// <summary>
        /// Overloads addition for bounds.
        /// </summary>
        public static Bounds operator +(Bounds bounds, Vector2 offset)
            => new(offset.X + bounds.Left, offset.Y + bounds.Top, offset.X + bounds.Right, offset.Y + bounds.Bottom);

        /// <summary>
        /// Overloads subtraction for bounds.
        /// </summary>
        public static Bounds operator -(Bounds bounds, Vector2 offset)
            => new(bounds.Left - offset.X, bounds.Top - offset.Y, bounds.Right - offset.X, bounds.Bottom - offset.Y);
    }
}
