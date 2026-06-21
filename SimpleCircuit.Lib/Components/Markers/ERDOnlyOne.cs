using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// An entity-relationship diagram marker for "only one".
/// </summary>
[Drawable("erd-only-one", "An Entity-Relationship Diagram only one-symbol.", "ERD")]
public class ERDOnlyOne : SegmentMarker
{
    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
    {
        builder.Line(new Vector2(-4, -3) * style.LineThickness, new Vector2(-4, 3) * style.LineThickness, style);
        builder.Line(new Vector2(-8, -3) * style.LineThickness, new Vector2(-8, 3) * style.LineThickness, style);
    }
}
