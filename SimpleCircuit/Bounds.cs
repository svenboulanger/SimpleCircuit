using System;

namespace SimpleCircuit
{
    public class Bounds
    {
        /// <summary>
        /// Gets the left coordinate.
        /// </summary>
        /// <value>
        /// The left coordinate.
        /// </value>
        public double Left { get; private set; }

        /// <summary>
        /// Gets the right coordinate.
        /// </summary>
        /// <value>
        /// The right coordinate.
        /// </value>
        public double Right { get; private set; }

        /// <summary>
        /// Gets the top coordinate.
        /// </summary>
        /// <value>
        /// The top coordinate.
        /// </value>
        public double Top { get; private set; }

        /// <summary>
        /// Gets the bottom coordinate.
        /// </summary>
        /// <value>
        /// The bottom coordinate.
        /// </value>
        public double Bottom { get; private set; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public double Width => Math.Abs(Right - Left);

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public double Height => Math.Abs(Bottom - Top);

        /// <summary>
        /// Expands the bounds looking at the specified point.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="extra">The extra space that needs to be reserved.</param>
        public void Expand(double x, double y, double extra = 1)
        {
            Left = Math.Min(x - extra, Left);
            Right = Math.Max(x + extra, Right);
            Bottom = Math.Max(y + extra, Bottom);
            Top = Math.Min(y - extra, Top);
        }

        /// <summary>
        /// Expands the bounds looking at the specified point.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="extra">The extra space that needs to be reserved.</param>
        public void Expand(Vector2 vector, double extra = 1)
            => Expand(vector.X, vector.Y, extra);
    }
}
