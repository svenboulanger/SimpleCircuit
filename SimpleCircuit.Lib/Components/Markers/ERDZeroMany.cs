using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Linq;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// An entity-relationship diagram marker for "zero or many".
/// </summary>
/// <remarks>
/// Creates a new entity-relationship diagram marker for "zero or many".
/// </remarks>
/// <param name="location">The location.</param>
/// <param name="orientation">The orientation.</param>
[Drawable("erd-zero-many", "An Entity-Relationship Diagram zero or many-symbol.", "ERD")]
public class ERDZeroMany(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
{
    private readonly static Vector2[] _points = [new(0, -3), new(-6, 0), new(0, 3)];

    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
    {
        // Just ot make sure
        style.RegisterVariable("bg-opaque", "white");

        builder.Polyline(_points.Select(p => p * style.LineThickness), style);
        builder.Circle(new Vector2(-9, 0) * style.LineThickness, 1.5, style.Color(null, "--bg-opaque"));
    }
}
