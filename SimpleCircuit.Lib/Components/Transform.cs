﻿using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A transformation for vectors.
    /// </summary>
    public struct Transform
    {
        private readonly Vector2 _origin;
        private readonly Matrix2 _orientation;

        /// <summary>
        /// Gets the identity transform.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        public static Transform Identity => new Transform(new(), Matrix2.Identity);

        public Transform(Vector2 origin, Matrix2 orientation)
        {
            _origin = origin;
            _orientation = orientation;
        }

        /// <summary>
        /// Applies the transform to a vector.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The transformed vector.</returns>
        public Vector2 Apply(Vector2 input)
            => _origin + _orientation * input;

        /// <summary>
        /// Applies a transform to another transform, resulting in the combined transform.
        /// </summary>
        /// <param name="tf">The first transform.</param>
        /// <returns>The combined transform.</returns>
        public Transform Apply(Transform tf)
        {
            var b = tf._orientation * _orientation;
            var o = tf.Apply(_origin);
            return new(o, b);
        }

        /// <summary>
        /// Applies the transform to a direction.
        /// </summary>
        /// <param name="input">The input direction.</param>
        /// <returns>The transformed direction.</returns>
        public Vector2 ApplyDirection(Vector2 input)
            => _orientation * input;

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