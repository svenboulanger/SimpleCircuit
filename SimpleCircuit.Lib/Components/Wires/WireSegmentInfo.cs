using SimpleCircuit.Components.Markers;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Parser;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Wires;

/// <summary>
/// Wire information.
/// </summary>
/// <remarks>
/// Creates a new <see cref="WireSegmentInfo"/>.
/// </remarks>
/// <param name="source">The source.</param>
public class WireSegmentInfo(TextLocation source)
{
    /// <summary>
    /// Gets the source of the wire segment.
    /// </summary>
    public TextLocation Source { get; } = source;

    /// <summary>
    /// Gets the angle of the wire.
    /// </summary>
    public Vector2 Orientation { get; set; }

    /// <summary>
    /// Determines whether the wire segment can be longer.
    /// </summary>
    public bool IsMinimum { get; set; }

    /// <summary>
    /// Determines whether the bounds should be used as an anchor.
    /// </summary>
    public bool UsesBounds { get; set; }

    /// <summary>
    /// Gets the length of the wire.
    /// </summary>
    public double Length { get; set; }

    /// <summary>
    /// Gets the variants specified on the wire segment.
    /// </summary>
    public HashSet<string> Variants { get; } = [];
}
