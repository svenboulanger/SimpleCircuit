using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// An abstract class representing a marker.
/// </summary>
public abstract class Marker
{
    /// <summary>
    /// Draws the marker to the given drawing.
    /// </summary>
    /// <param name="builder">The graphics builder.</param>
    /// <param name="style">The marker style.</param>
    public abstract void Draw(IGraphicsBuilder builder, IStyle style,
        IReadOnlyList<SegmentInfo> segments);
}
