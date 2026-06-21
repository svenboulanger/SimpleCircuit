using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Linq;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// An entity-relationship diagram marker for "many".
/// </summary>
[Drawable("erd-many", "An Entity-Relationship Diagram many-symbol.", "ERD")]
public class ERDMany : SegmentMarker
{
    private readonly static Vector2[] _points = [new(0, -1.5), new(-3, 0), new(0, 1.5)];

    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
    {
        builder.Polyline(_points.Select(p => p * 2.0 * style.LineThickness), style);
        builder.Line(new Vector2(-4, 0) * style.LineThickness, new(), style);
    }
}
