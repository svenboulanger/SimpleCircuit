using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A token.
    /// </summary>
    public readonly struct Token : IEquatable<Token>
    {
        /// <summary>
        /// Gets the source of the token.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the line number of the token.
        /// </summary>
        public TextLocation Location { get; }

        /// <summary>
        /// Gets the contents of the token.
        /// </summary>
        public ReadOnlyMemory<char> Content { get; }

        /// <summary>
        /// Creates a new <see cref="Token"/>
        /// </summary>
        /// <param name="source">The source of the token.</param>
        /// <param name="location">The location of the token.</param>
        /// <param name="content">The contents of the token.</param>
        public Token(string source, TextLocation location, ReadOnlyMemory<char> content)
        {
            Source = source;
            Location = location;
            Content = content;
        }

        /// <inheritdoc />
        public bool Equals(Token other) => Source.Equals(other.Source) && Location.Equals(other.Location) && Content.Equals(other.Content);

        /// <inheritdoc />
        public override string ToString() => $"{Content} in {Source ?? ""} ({Location})";
    }
}