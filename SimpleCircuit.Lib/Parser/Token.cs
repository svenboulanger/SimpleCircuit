using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A token.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="Token"/>
    /// </remarks>
    /// <param name="location">The location of the token.</param>
    /// <param name="content">The contents of the token.</param>
    public readonly struct Token(TextLocation location, ReadOnlyMemory<char> content) : IEquatable<Token>
    {
        /// <summary>
        /// Gets the line number of the token.
        /// </summary>
        public TextLocation Location { get; } = location;

        /// <summary>
        /// Gets the contents of the token.
        /// </summary>
        public ReadOnlyMemory<char> Content { get; } = content;

        /// <inheritdoc />
        public bool Equals(Token other) => Location.Equals(other.Location) && Content.Equals(other.Content);

        /// <inheritdoc />
        public override string ToString() => $"{Content} ({Location})";

        /// <summary>
        /// Allows implicit conversion from a token to a text location.
        /// </summary>
        /// <param name="token">The text location.</param>
        public static implicit operator TextLocation(Token token) => token.Location;
    }
}