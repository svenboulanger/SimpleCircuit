using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// A dot marker.
/// </summary>
/// <remarks>
/// Creates a new dot marker.
/// </remarks>
[Drawable("dot", "A generic dot.", "General")]
public class Dot : SegmentMarker
{
    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
    {
        builder.Circle(new(), 2.0 * style.LineThickness, style.AsFilledMarker());
    }
}
