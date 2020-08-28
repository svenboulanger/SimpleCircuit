using System;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// A simple helper struct for calculating offset matrix contributions. This means that:
    /// <list type="bullet">
    /// <item>f_x = x + x_r * cos(angle) - y_r * sin(angle)</item>
    /// <item>f_y = y + y_r * sin(angle) + y_r * cos(angle)</item>
    /// </list>
    /// </summary>
    public struct OffsetContributions
    {
        public readonly double Dfxda;
        public readonly double Dfyda;
        public readonly double Frx;
        public readonly double Fry;

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetContributions"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="offset">The offset.</param>
        public OffsetContributions(double angle, Vector2 offset)
        {
            var c = Math.Cos(angle);
            var s = Math.Sin(angle);
            Frx = offset.X * c - offset.Y * s;
            Fry = offset.X * s + offset.Y * c;
            Dfxda = -offset.X * s - offset.Y * c;
            Dfyda = offset.X * c - offset.Y * s;
        }
    }
}
