using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// An entity-relation diagram marker for "zero or one".
/// </summary>
[Drawable("erd-zero-one", "An Entity-Relationship Diagram zero or one-symbol.", "ERD")]
public class ERDZeroOne : SegmentMarker
{
    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
    {
        // Just ot make sure
        style.RegisterVariable("bg-opaque", "white");

        builder.Circle(new(-11 * style.LineThickness, 0), 3 * style.LineThickness, style.Color(null, "--bg-opaque"));
        builder.Line(new Vector2(-4, -3) * style.LineThickness, new Vector2(-4, 3) * style.LineThickness, style);
    }
}
