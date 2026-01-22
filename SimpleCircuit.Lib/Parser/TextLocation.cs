using System;

namespace SimpleCircuit.Parser;

/// <summary>
/// Represents a text location.
/// </summary>
/// <remarks>
/// Creates a new <see cref="TextLocation"/>.
/// </remarks>
/// <param name="line">The line number.</param>
/// <param name="column">The column number.</param>
public readonly struct TextLocation(string source, int line, int column) : IEquatable<TextLocation>
{
    /// <summary>
    /// Gets the source.
    /// </summary>
    public string Source { get; } = source ?? string.Empty;

    /// <summary>
    /// Gets the line number.
    /// </summary>
    public int Line { get; } = line;

    /// <summary>
    /// Gets the column number.
    /// </summary>
    public int Column { get; } = column;

    /// <inheritdoc />
    public bool Equals(TextLocation other) => Source == other.Source && Line == other.Line && Column == other.Column;

    /// <inheritdoc />
    public override string ToString() => string.IsNullOrEmpty(Source) ? $"line {Line}, column {Column}" : $"line {Line}, column {Column} in {Source}";
}