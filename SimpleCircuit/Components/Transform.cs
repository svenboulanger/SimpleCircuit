using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A transformation for vectors.
    /// </summary>
    public struct Transform
    {
        private readonly Vector2 _origin;
        private readonly double _a11, _a12, _a21, _a22;

        /// <summary>
        /// Gets the identity transform.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        public static Transform Identity => new Transform(0.0, 0.0, new Vector2(1, 0), new Vector2(0, 1));

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> struct.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="nx">The nx.</param>
        /// <param name="ny">The ny.</param>
        public Transform(double x, double y, Vector2 nx, Vector2 ny)
        {
            _origin = new Vector2(x, y);
            _a11 = nx.X;
            _a21 = nx.Y;
            _a12 = ny.X;
            _a22 = ny.Y;
        }

        /// <summary>
        /// Applies the transform to a vector.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The transformed vector.</returns>
        public Vector2 Apply(Vector2 input)
        {
            return new Vector2(
                _origin.X + _a11 * input.X + _a12 * input.Y,
                _origin.Y + _a21 * input.X + _a22 * input.Y
                );
        }

        /// <summary>
        /// Applies a transform to another transform, resulting in the combined transform.
        /// </summary>
        /// <param name="tf">The first transform.</param>
        /// <returns>The combined transform.</returns>
        public Transform Apply(Transform tf)
        {
            // Transform the normal directions
            var nx = tf.ApplyDirection(new Vector2(_a11, _a21));
            var ny = tf.ApplyDirection(new Vector2(_a12, _a22));

            // Transform the position
            var pos = tf.Apply(new Vector2(_origin.X, _origin.Y));
            return new Transform(pos.X, pos.Y, nx, ny);
        }

        /// <summary>
        /// Applies the transform to a direction.
        /// </summary>
        /// <param name="input">The input direction.</param>
        /// <returns>The transformed direction.</returns>
        public Vector2 ApplyDirection(Vector2 input)
        {
            return new Vector2(
                _a11 * input.X + _a12 * input.Y,
                _a21 * input.X + _a22 * input.Y);
        }

        /// <summary>
        /// Applies the transform to the specified inputs.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <returns>The transformed inputs.</returns>
        public IEnumerable<Vector2> Apply(IEnumerable<Vector2> inputs)
        {
            foreach (var input in inputs)
                yield return Apply(input);
        }
    }
}
