using System;

namespace SimpleCircuit
{
    /// <summary>
    /// A 2D vector.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Vector2"/> struct.
    /// </remarks>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    public struct Vector2(double x, double y) : IEquatable<Vector2>
    {
        private const int _precision = 9;
        private const double _dblPrecision = 1e-9;

        /// <summary>
        /// Gets the x-coordinate.
        /// </summary>
        /// <value>
        /// The x-coordinate.
        /// </value>
        public double X { get; } = x;

        /// <summary>
        /// Gets the y-coordinate.
        /// </summary>
        /// <value>
        /// The y-coordinate.
        /// </value>
        public double Y { get; } = y;

        /// <summary>
        /// Gets the vector that is perpendicular and of the same length.
        /// </summary>
        /// <value>
        /// The perpendicular vector.
        /// </value>
        public readonly Vector2 Perpendicular => new(-Y, X);

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public readonly double Length => Math.Sqrt(X * X + Y * Y);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override readonly int GetHashCode()
        {
            int hash = Math.Round(X, _precision).GetHashCode();
            hash = (hash * 13) ^ Math.Round(Y, _precision).GetHashCode();
            return hash;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is Vector2 vec)
                return Equals(vec);
            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public readonly bool Equals(Vector2 other)
        {
            if (Math.Abs(other.X - X) > _dblPrecision)
                return false;
            if (Math.Abs(other.Y - Y) > _dblPrecision)
                return false;
            return true;
        }

        /// <summary>
        /// Computes the dot-product with another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The dot product.</returns>
        public readonly double Dot(Vector2 other) => X * other.X + Y * other.Y;

        /// <summary>
        /// Rotates the vector.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns>The rotated vector.</returns>
        public readonly Vector2 Rotate(double angle)
        {
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            return new Vector2(X * c - Y * s, X * s + Y * c);
        }

        /// <summary>
        /// Scales the vector.
        /// </summary>
        /// <param name="sx">The scaling along the x-axis.</param>
        /// <param name="sy">The scaling along the y-axis.</param>
        /// <returns>The scaled vector.</returns>
        public readonly Vector2 Scale(double sx, double sy)
        {
            return new Vector2(X * sx, Y * sy);
        }

        /// <summary>
        /// Returns the normal under the specified angle.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns>The normal.</returns>
        public static Vector2 Normal(double angle) => new(Math.Cos(angle), Math.Sin(angle));

        /// <summary>
        /// Orders nodes according to the direction of the vector. The returned vector is
        /// the vector pointing into the first quadrant. The arguments are ordered approprately.
        /// </summary>
        /// <param name="lowestX">The lowest node X.</param>
        /// <param name="highestX">The highest node X.</param>
        /// <param name="lowestY">The lowest node Y.</param>
        /// <param name="highestY">The highest node Y.</param>
        /// <returns>The vector in the first quadrant.</returns>
        public readonly Vector2 Order<T>(ref T lowestX, ref T highestX, ref T lowestY, ref T highestY)
        {
            T tmp;
            double rx, ry;
            if (X < 0)
            {
                tmp = lowestX;
                lowestX = highestX;
                highestX = tmp;
                rx = -X;
            }
            else
                rx = X;
            if (Y < 0)
            {
                tmp = lowestY;
                lowestY = highestY;
                highestY = tmp;
                ry = -Y;
            }
            else
                ry = Y;
            return new(rx, ry);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override readonly string ToString()
        {
            return $"({X:G4}, {Y:G4})";
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator -(Vector2 a) => new(-a.X, -a.Y);

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="f">The factor.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator *(Vector2 a, double f) => new(a.X * f, a.Y * f);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static double operator *(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="f">The factor.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator /(Vector2 a, double f) => new(a.X / f, a.Y / f);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="f">The factor.</param>
        /// <param name="a">The first argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator *(double f, Vector2 a) => a * f;
    }
}
