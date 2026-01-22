namespace SimpleCircuit.Components.Wires;

/// <summary>
/// Describes a wire segment with a start and end.
/// </summary>
/// <remarks>
/// Creates a new <see cref="WireSegment"/>.
/// </remarks>
/// <param name="start">The start of the segment.</param>
/// <param name="end">The end of the segment.</param>
public readonly struct WireSegment(Vector2 start, Vector2 end)
{
    /// <summary>
    /// Gets the start of the wire segment.
    /// </summary>
    public Vector2 Start { get; } = start;

    /// <summary>
    /// Gets the end of the wire segment.
    /// </summary>
    public Vector2 End { get; } = end;

    /// <inheritdoc />
    public override string ToString() => $"{Start} -> {End}";
}
