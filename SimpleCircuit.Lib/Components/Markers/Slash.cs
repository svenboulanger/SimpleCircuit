using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers;

/// <summary>
/// A slash marker.
/// </summary>
/// <remarks>
/// Creates a new slash marker.
/// </remarks>
/// <param name="location">The location.</param>
/// <param name="orientation">The orientation.</param>
[Drawable("slash", "A generic slash marker.", "General")]
public class Slash(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
{
    /// <inheritdoc />
    protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
        => builder.Line(new(-1, 2), new(1, -2), style);
}
