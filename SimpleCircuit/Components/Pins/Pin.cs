using System;
using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A pin that has a direction and can translate.
    /// </summary>
    /// <seealso cref="TranslatingPin" />
    /// <seealso cref="IRotating" />
    public class RotatingPin : TranslatingPin, IRotating
    {
         /// <inheritdoc/>
        public Function NormalX { get; }

        /// <inheritdoc/>
        public Function NormalY { get; }

        /// <inheritdoc/>
        public Function Angle { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RotatingPin"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="nx">The nx.</param>
        /// <param name="ny">The ny.</param>
        public RotatingPin(string name, string description, IComponent owner, Function x, Function y, Function nx, Function ny)
            : base(name, description, owner, x, y)
        {
            NormalX = nx ?? throw new ArgumentNullException(nameof(nx));
            NormalY = ny ?? throw new ArgumentNullException(nameof(ny));
            Angle = new NormalAtan2(ny, nx);
        }
    }
}
