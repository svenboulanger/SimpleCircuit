using SimpleCircuit.Functions;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A pin that has a position.
    /// </summary>
    public class TranslatingPin : IPin, ITranslating
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Description { get; }

        /// <inheritdoc/>
        public IComponent Owner { get; }

        /// <inheritdoc/>
        public Function X { get; }

        /// <inheritdoc/>
        public Function Y { get; }

        /// <summary>
        /// Creates a translating pin.
        /// </summary>
        /// <param name="name">The pin name.</param>
        /// <param name="description">The pin description.</param>
        /// <param name="owner">The pin owner.</param>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public TranslatingPin(string name, string description, IComponent owner, Function x, Function y)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            X = x ?? throw new ArgumentNullException(nameof(x));
            Y = y ?? throw new ArgumentNullException(nameof(y));
        }

        /// <summary>
        /// Convert to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"{Owner.Name}.{Name}";
    }
}
