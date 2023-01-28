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
        /// Gets the range of the token.
        /// </summary>
        public TextRange Range { get; }

        /// <summary>
        /// Gets the contents of the token.
        /// </summary>
        public ReadOnlyMemory<char> Content { get; }

        /// <summary>
        /// Creates a new <see cref="Token"/>
        /// </summary>
        /// <param name="source">The source of the token.</param>
        /// <param name="range">The range of the token.</param>
        /// <param name="content">The contents of the token.</param>
        public Token(string source, TextRange range, ReadOnlyMemory<char> content)
        {
            Source = source;
            Range = range;
            Content = content;
        }

        /// <inheritdoc />
        public bool Equals(Token other) => Source.Equals(other.Source) && Range.Equals(other.Range) && Content.Equals(other.Content);

        /// <inheritdoc />
        public override string ToString() => $"{Content} in {Source ?? ""} ({Range})";
    }
}