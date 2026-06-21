using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// A plus marker.
/// </summary>
[Drawable("plus", "A generic plus symbol.", "General")]
[Drawable("plusa", "A generic plus symbol.", "General")]
public class Plus : SegmentMarker
{
    /// <summary>
    /// Gets whether the plus should be drawn on the opposite side.
    /// </summary>
    public bool OppositeSide { get; set; }

    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
    {
        Vector2 offset = OppositeSide ? new(-2.5, 3) : new(-2.5, -3);
        builder.BeginTransform(new(offset, builder.CurrentTransform.Matrix.Inverse));
        builder.Line(new(-1, 0), new(1, 0), style);
        builder.Line(new(0, -1), new(0, 1), style);
        builder.EndTransform();
    }
}
