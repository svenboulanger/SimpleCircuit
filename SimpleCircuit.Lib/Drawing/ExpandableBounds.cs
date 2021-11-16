using System;
using System.Collections.Generic;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Bounds that can be expanded.
    /// </summary>
    public class ExpandableBounds
    {
        private double _l = 0, _r = 0, _t = 0, _b = 0;
        
        /// <summary>
        /// Gets the bounds.
        /// </summary>
        public Bounds Bounds
        {
            get
            {
                return new(_l, _t, _r, _b);
            }
        }

        /// <summary>
        /// Expands the bounds looking at the specified point.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public void Expand(double x, double y)
        {
            _l = Math.Min(x, _l);
            _r = Math.Max(x, _r);
            _b = Math.Max(y, _b);
            _t = Math.Min(y, _t);
        }

        /// <summary>
        /// Expands the bounds looking at the specified point.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public void Expand(Vector2 vector)
            => Expand(vector.X, vector.Y);

        /// <summary>
        /// Expands the bounds looking at the specified points.
        /// </summary>
        /// <param name="vectors">The vector.</param>
        public void Expand(IEnumerable<Vector2> vectors)
        {
            foreach (var v in vectors)
                Expand(v);
        }

        /// <summary>
        /// Converts the bounds to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => Bounds.ToString();
    }
}
