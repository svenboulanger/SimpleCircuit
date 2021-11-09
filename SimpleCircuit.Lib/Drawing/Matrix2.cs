using System;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// a 2D matrix.
    /// </summary>
    public struct Matrix2 : IEquatable<Matrix2>
    {
        private const int _precision = 9;
        private const double _dblPrecision = 1e-9;

        /// <summary>
        /// Gets the identity matrix.
        /// </summary>
        public static Matrix2 Identity => new(1, 0, 0, 1);

        /// <summary>
        /// Gets the top-left coefficient.
        /// </summary>
        public double A11 { get; }

        /// <summary>
        /// Gets the top-right coefficient.
        /// </summary>
        public double A12 { get; }

        /// <summary>
        /// Gets the bottom-left coefficient.
        /// </summary>
        public double A21 { get; }

        /// <summary>
        /// Gets the bottom-right coefficient.
        /// </summary>
        public double A22 { get; }

        /// <summary>
        /// Gets the determinant of the matrix.
        /// </summary>
        public double Determinant => A11 * A22 - A12 * A21;

        /// <summary>
        /// Gets the inverse of the matrix.
        /// </summary>
        public Matrix2 Inverse
        {
            get
            {
                double determinant = Determinant;
                return new(A22 / determinant, -A12 / determinant,
                    -A21 / determinant, A11 / determinant);
            }
        }

        /// <summary>
        /// Gets the transposed matrix.
        /// </summary>
        public Matrix2 Transposed => new(A11, A21, A12, A22);

        /// <summary>
        /// Gets whether the matrix is orthonormal. This means that the column vectors
        /// are all orthonormal to each other.
        /// </summary>
        public bool IsOrthonormal
        {
            get
            {
                if (!(A11 * A12 + A21 * A22).IsZero())
                    return false;
                if (!(A11 * A11 + A21 * A21 - 1).IsZero())
                    return false;
                if (!(A12 * A12 + A22 * A22 - 1).IsZero())
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Matrix2"/> struct.
        /// </summary>
        /// <param name="a11">The top-left coefficient.</param>
        /// <param name="a12">The top-right coefficient.</param>
        /// <param name="a21">The bottom-left coefficient.</param>
        /// <param name="a22">The bottom-right coefficient.</param>
        public Matrix2(double a11, double a12, double a21, double a22)
        {
            A11 = a11;
            A12 = a12;
            A21 = a21;
            A22 = a22;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int hash = Math.Round(A11, _precision).GetHashCode();
            hash = hash * 13 ^ Math.Round(A12, _precision).GetHashCode();
            hash = hash * 13 ^ Math.Round(A21, _precision).GetHashCode();
            hash = hash * 13 ^ Math.Round(A22, _precision).GetHashCode();
            return hash;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Matrix2 mat)
                return Equals(mat);
            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Matrix2 other)
        {
            if (Math.Abs(A11 - other.A11) > _dblPrecision)
                return false;
            if (Math.Abs(A22 - other.A22) > _dblPrecision)
                return false;
            if (Math.Abs(A12 - other.A12) > _dblPrecision)
                return false;
            if (Math.Abs(A21 - other.A21) > _dblPrecision)
                return false;
            return true;
        }

        /// <summary>
        /// Tries to invert the matrix.
        /// </summary>
        /// <param name="inversion">The inverted matrix.</param>
        /// <returns>Returns <c>true</c> if the matrix could be inverted; otherwise, <c>false</c>.</returns>
        public bool TryInvert(out Matrix2 inversion)
        {
            double determinant = Determinant;
            if (determinant.IsZero())
            {
                inversion = new();
                return false;
            }
            else
            {
                inversion = new(A22 / determinant, -A12 / determinant,
                    -A21 / determinant, A11 / determinant);
                return true;
            }
        }

        /// <summary>
        /// Creates a rotation matrix.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns>The rotating matrix.</returns>
        public static Matrix2 Rotate(double angle)
        {
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            return new(c, -s, s, c);
        }

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        /// <param name="sx">The scaling factor along the X-direction.</param>
        /// <param name="sy">The scaling factor along the Y-direction.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix2 Scale(double sx, double sy)
            => new(sx, 0, 0, sy);

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        /// <param name="s">The scaling factor.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix2 Scale(double s)
            => new(s, 0, 0, s);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"({A11:G4}, {A12:G4}; {A21:G4}, {A22:G4})";
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Matrix2 operator -(Matrix2 a) => new(-a.A11, -a.A12, -a.A21, -a.A22);

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Matrix2 operator +(Matrix2 a, Matrix2 b)
            => new(a.A11 + b.A11, a.A12 + b.A12, a.A21 + b.A21, a.A22 + b.A22);

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Matrix2 operator -(Matrix2 a, Matrix2 b)
            => new(a.A11 - b.A11, a.A12 - b.A12, a.A21 - b.A21, a.A22 - b.A22);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Matrix2 operator *(Matrix2 a, Matrix2 b)
        {
            return new(
                a.A11 * b.A11 + a.A12 * b.A21, a.A11 * b.A12 + a.A12 * b.A22,
                a.A21 * b.A11 + a.A22 * b.A21, a.A21 * b.A12 + a.A22 * b.A22);
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector2 operator *(Matrix2 a, Vector2 b)
        {
            return new(
                a.A11 * b.X + a.A12 * b.Y,
                a.A21 * b.X + a.A22 * b.Y);
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="f">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Matrix2 operator /(Matrix2 a, double f)
            => new(a.A11 / f, a.A12 / f, a.A21 / f, a.A22 / f);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="f">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Matrix2 operator *(Matrix2 a, double f)
            => new(a.A11 * f, a.A12 * f, a.A21 * f, a.A22 * f);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="f">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Matrix2 operator *(double f, Matrix2 b)
            => new(f * b.A11, f * b.A12, f * b.A21, f * b.A22);
    }
}
