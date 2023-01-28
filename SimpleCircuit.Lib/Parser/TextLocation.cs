using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Represents a text location.
    /// </summary>
    public readonly struct TextLocation : IEquatable<TextLocation>
    {
        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the column number.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Creates a new <see cref="TextLocation"/>.
        /// </summary>
        /// <param name="line">The line number.</param>
        /// <param name="column">The column number.</param>
        public TextLocation(int line, int column)
        {
            Line = line;
            Column = column;
        }

        /// <inheritdoc />
        public bool Equals(TextLocation other) => Line == other.Line && Column == other.Column;

        /// <inheritdoc />
        public override string ToString() => $"line {Line}, column {Column}";
    }
}