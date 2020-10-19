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
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the pin.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; }

        /// <summary>
        /// Gets the component that this pin is part of.
        /// </summary>
        public IComponent Owner { get; }

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
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="nx">The nx.</param>
        /// <param name="ny">The ny.</param>
        public Pin(string name, string description, IComponent owner, Function x, Function y, Function nx, Function ny)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            X = x ?? throw new ArgumentNullException(nameof(x));
            Y = y ?? throw new ArgumentNullException(nameof(y));
            NormalX = nx ?? throw new ArgumentNullException(nameof(nx));
            NormalY = ny ?? throw new ArgumentNullException(nameof(ny));
            Angle = new NormalAtan2(ny, nx);
        }
    }
}
