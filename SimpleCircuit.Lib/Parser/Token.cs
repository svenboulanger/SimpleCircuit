using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A token.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="Token"/>
    /// </remarks>
    /// <param name="source">The source of the token.</param>
    /// <param name="location">The location of the token.</param>
    /// <param name="content">The contents of the token.</param>
    public readonly struct Token(string source, TextLocation location, ReadOnlyMemory<char> content) : IEquatable<Token>
    {
        /// <summary>
        /// Gets the source of the token.
        /// </summary>
        public string Source { get; } = source;

        /// <summary>
        /// Gets the line number of the token.
        /// </summary>
        public TextLocation Location { get; } = location;

        /// <summary>
        /// Gets the contents of the token.
        /// </summary>
        public ReadOnlyMemory<char> Content { get; } = content;

        /// <inheritdoc />
        public bool Equals(Token other) => Source.Equals(other.Source) && Location.Equals(other.Location) && Content.Equals(other.Content);

        /// <inheritdoc />
        public override string ToString() => $"{Content} in {Source ?? ""} ({Location})";
    }
}