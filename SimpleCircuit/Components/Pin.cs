using System;
using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A pin.
    /// </summary>
    /// <seealso cref="ITranslating" />
    /// <seealso cref="IRotating" />
    public class Pin : ITranslating, IRotating
    {
        /// <summary>
        /// Gets the description of the pin.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; }

        /// <inheritdoc/>
        public Function X { get; }

        /// <inheritdoc/>
        public Function Y { get; }

        /// <inheritdoc/>
        public Function NormalX { get; }

        /// <inheritdoc/>
        public Function NormalY { get; }

        /// <inheritdoc/>
        public Function Angle { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pin"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="nx">The nx.</param>
        /// <param name="ny">The ny.</param>
        public Pin(string description, Function x, Function y, Function nx, Function ny)
        {
            Description = description;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            X = x ?? throw new ArgumentNullException(nameof(x));
            Y = y ?? throw new ArgumentNullException(nameof(y));
            NormalX = nx ?? throw new ArgumentNullException(nameof(nx));
            NormalY = ny ?? throw new ArgumentNullException(nameof(ny));
            Angle = new NormalAtan2(ny, nx);
        }
    }
}
