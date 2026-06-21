using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// A slash marker.
/// </summary>
[Drawable("slash", "A generic slash marker.", "General")]
public class Slash : SegmentMarker
{
    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
        => builder.Line(new(-1, 2), new(1, -2), style);
}
