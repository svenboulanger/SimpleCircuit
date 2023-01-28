using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A text range.
    /// </summary>
    public readonly struct TextRange : IEquatable<TextRange>
    {
        /// <summary>
        /// Gets the start of the range.
        /// </summary>
        public TextLocation Start { get; }

        /// <summary>
        /// Gets the end of the range.
        /// </summary>
        public TextLocation End { get; }

        /// <summary>
        /// Creates a new <see cref="TextRange"/>.
        /// </summary>
        /// <param name="start">The start of the range.</param>
        /// <param name="end">The end of the range.</param>
        public TextRange(TextLocation start, TextLocation end)
        {
            Start = start;
            End = end;
        }

        /// <inheritdoc />
        public bool Equals(TextRange other) => Start.Equals(other.Start) && End.Equals(other.End);

        /// <inheritdoc />
        public override string ToString()
        {
            if (Start.Line == End.Line)
            {
                if (Start.Column == End.Column)
                    return $"{Start.Line},{Start.Column}";
                return $"{Start.Line},{Start.Column}-{End.Column}";
            }
            else
                return $"{Start.Line},{Start.Column}-{End.Line},{End.Column}";
        }
    }
}