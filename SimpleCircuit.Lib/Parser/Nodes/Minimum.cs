using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A node that represents a minimum distance.
    /// </summary>
    public record MinimumNode : SyntaxNode
    {
        /// <summary>
        /// The distance.
        /// </summary>
        public SyntaxNode Distance { get; }

        /// <summary>
        /// Creates a new <see cref="MinimumNode"/>.
        /// </summary>
        /// <param name="distance">The distance.</param>
        /// <param name="location">The location.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public MinimumNode(SyntaxNode distance, TextLocation location)
            : base(location)
        {
            Distance = distance ?? throw new ArgumentNullException(nameof(distance));
        }

        /// <inheritdoc />
        public override string ToString() => $"min({Distance})";
    }
}
