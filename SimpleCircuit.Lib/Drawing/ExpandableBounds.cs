using System;
using System.Collections.Generic;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Bounds that can be expanded.
    /// </summary>
    public class ExpandableBounds
    {
        private double _l = double.MaxValue, _r = double.MinValue, _t = double.MaxValue, _b = double.MinValue;
        
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
        /// Resets the expandable bounds.
        /// </summary>
        public void Reset()
        {
            _l = double.MaxValue;
            _r = double.MinValue;
            _t = double.MaxValue;
            _b = double.MinValue;
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
        /// Expands the bounds looking at the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        public void Expand(ExpandableBounds bounds)
        {
            _l = Math.Min(bounds._l, _l);
            _r = Math.Max(bounds._r, _r);
            _t = Math.Min(bounds._t, _t);
            _b = Math.Max(bounds._b, _b);
        }

        /// <summary>
        /// Expands the bounds looking at the specified bounds.
        /// </summary>
        /// <param name="bounds"></param>
        public void Expand(Bounds bounds)
        {
            _l = Math.Min(bounds.Left, _l);
            _r = Math.Max(bounds.Right, _r);
            _t = Math.Min(bounds.Top, _t);
            _b = Math.Max(bounds.Bottom, _b);
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
        /// Expands the bounds looking at the specified points.
        /// </summary>
        /// <param name="vectors"></param>
        public void Expand(params Vector2[] vectors)
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
