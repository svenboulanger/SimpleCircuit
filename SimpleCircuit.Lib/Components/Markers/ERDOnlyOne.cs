using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// An entity-relationship diagram marker for "only one".
/// </summary>
/// <remarks>
/// Creates an entity-relationship diagram marker for "only one".
/// </remarks>
/// <param name="location">The location.</param>
/// <param name="orientation">The orientation.</param>
[Drawable("erd-only-one", "An Entity-Relationship Diagram only one-symbol.", "ERD")]
public class ERDOnlyOne(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
{
    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
    {
        builder.Line(new Vector2(-4, -3) * style.LineThickness, new Vector2(-4, 3) * style.LineThickness, style);
        builder.Line(new Vector2(-8, -3) * style.LineThickness, new Vector2(-8, 3) * style.LineThickness, style);
    }
}
