using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// An entity-relationship diagram marker for "one".
/// </summary>
/// <remarks>
/// Creates a new entity-relationship diagram marker for "one".
/// </remarks>
/// <param name="location">The location.</param>
/// <param name="orientation">The orientation.</param>
[Drawable("erd-one", "An Entity-Relationship Diagram one-symbol.", "ERD")]
public class ERDOne(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
{
    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
    {
        builder.Line(new Vector2(-4, -3) * style.LineThickness, new Vector2(-4, 3) * style.LineThickness, style);
    }
}
