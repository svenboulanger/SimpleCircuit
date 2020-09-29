using System;

namespace SimpleCircuit
{
    /// <summary>
    /// A 2D vector.
    /// </summary>
    public struct Vector2 : IEquatable<Vector2>
    {
        /// <summary>
        /// Gets the x-coordinate.
        /// </summary>
        /// <value>
        /// The x-coordinate.
        /// </value>
        public double X { get; }

        /// <summary>
        /// Gets the y-coordinate.
        /// </summary>
        /// <value>
        /// The y-coordinate.
        /// </value>
        public double Y { get; }

        /// <summary>
        /// Gets the vector that is perpendicular and of the same length.
        /// </summary>
        /// <value>
        /// The perpendicular vector.
        /// </value>
        public Vector2 Perpendicular => new Vector2(-Y, X);

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public double Length => Math.Sqrt(X * X + Y * Y);

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (X.GetHashCode() * 13) ^ Y.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
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
        public bool Equals(Vector2 other)
        {
            if (Math.Abs(other.X - X) > 1e-9)
                return false;
            if (Math.Abs(other.Y - Y) > 1e-9)
                return false;
            return true;
        }

        /// <summary>
        /// Rotates the vector.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns>The rotated vector.</returns>
        public Vector2 Rotate(double angle)
        {
            var c = Math.Cos(angle);
            var s = Math.Sin(angle);
            return new Vector2(X * c - Y * s, X * s + Y * c);
        }

        /// <summary>
        /// Scales the vector.
        /// </summary>
        /// <param name="sx">The scaling along the x-axis.</param>
        /// <param name="sy">The scaling along the y-axis.</param>
        /// <returns>The scaled vector.</returns>
        public Vector2 Scale(double sx, double sy)
        {
            return new Vector2(X * sx, Y * sy);
        }

        /// <summary>
        /// Returns the normal under the specified angle.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns>The normal.</returns>
        public static Vector2 Normal(double angle) => new Vector2(Math.Cos(angle), Math.Sin(angle));

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
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
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="f">The factor.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator *(Vector2 a, double f) => new Vector2(a.X * f, a.Y * f);

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
        public static Vector2 operator /(Vector2 a, double f) => new Vector2(a.X / f, a.Y / f);

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
