using System;
using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    public class Pin
    {
        /// <summary>
        /// Gets the x-coordinate of the pin.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public Function X { get; }

        /// <summary>
        /// Gets the y-coordinate of the pin.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public Function Y { get; }

        /// <summary>
        /// Gets the normal x-coordinate.
        /// </summary>
        /// <value>
        /// The normal x.
        /// </value>
        public Function NormalX { get; }

        /// <summary>
        /// Gets the normal y.
        /// </summary>
        /// <value>
        /// The normal y.
        /// </value>
        public Function NormalY { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pin"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="nx">The nx.</param>
        /// <param name="ny">The ny.</param>
        public Pin(Function x, Function y, Function nx, Function ny)
        {
            X = x ?? throw new ArgumentNullException(nameof(x));
            Y = y ?? throw new ArgumentNullException(nameof(y));
            NormalX = nx ?? throw new ArgumentNullException(nameof(nx));
            NormalY = ny ?? throw new ArgumentNullException(nameof(ny));
        }
    }
}
