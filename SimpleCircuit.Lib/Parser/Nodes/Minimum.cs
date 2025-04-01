using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A node that represents a minimum distance.
    /// </summary>
    public record Minimum : SyntaxNode
    {
        /// <summary>
        /// The distance.
        /// </summary>
        public SyntaxNode Distance { get; }

        /// <summary>
        /// Creates a new <see cref="Minimum"/>.
        /// </summary>
        /// <param name="distance">The distance.</param>
        /// <param name="location">The location.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Minimum(SyntaxNode distance, TextLocation location)
            : base(location)
        {
            Distance = distance ?? throw new ArgumentNullException(nameof(distance));
        }

        /// <inheritdoc />
        public override string ToString() => $"min({Distance})";
    }
}
