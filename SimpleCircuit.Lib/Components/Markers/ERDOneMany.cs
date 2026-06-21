using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Linq;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// An entity-relationship diagram marker for "one or many".
/// </summary>
[Drawable("erd-one-many", "An Entity-Relationship Diagram one or many-symbol.", "ERD")]
public class ERDOneMany : SegmentMarker
{
    private readonly static Vector2[] _points = [new(0, -3), new(-6, 0), new(0, 3)];

    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
    {
        builder.Polyline(_points.Select(p => p * style.LineThickness), style);
        builder.Line(new Vector2(-6, -3) * style.LineThickness, new Vector2(-6, 3) * style.LineThickness, style);
    }
}
