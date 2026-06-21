namespace SimpleCircuit.Components.Markers;

/// <summary>
/// Information about a segment.
/// </summary>
/// <param name="start">The starting point of the segment.</param>
/// <param name="startNormal">The starting normal of the segment.</param>
/// <param name="end">The end point of the segment.</param>
/// <param name="endNormal">The end normal of the segment.</param>
public readonly struct SegmentInfo(Vector2 start, Vector2 startNormal, Vector2 end, Vector2 endNormal)
{
    /// <summary>
    /// Gets the starting point of the segment.
    /// </summary>
    public Vector2 Start { get; } = start;

    /// <summary>
    /// Gets the orientation of the starting point of the segment.
    /// </summary>
    public Vector2 StartNormal { get; } = startNormal;

    /// <summary>
    /// Gets the ending point of the segment.
    /// </summary>
    public Vector2 End { get; } = end;

    /// <summary>
    /// Gets the orientation of the ending point of the segment.
    /// </summary>
    public Vector2 EndNormal { get; } = endNormal;
}
