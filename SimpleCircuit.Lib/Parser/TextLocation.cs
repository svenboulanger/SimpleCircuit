using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Represents a text location.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="TextLocation"/>.
    /// </remarks>
    /// <param name="line">The line number.</param>
    /// <param name="column">The column number.</param>
    public readonly struct TextLocation(int line, int column) : IEquatable<TextLocation>
    {
        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int Line { get; } = line;

        /// <summary>
        /// Gets the column number.
        /// </summary>
        public int Column { get; } = column;

        /// <inheritdoc />
        public bool Equals(TextLocation other) => Line == other.Line && Column == other.Column;

        /// <inheritdoc />
        public override string ToString() => $"line {Line}, column {Column}";
    }
}