using SimpleCircuit.Components.Styles;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// A dot marker.
    /// </summary>
    /// <remarks>
    /// Creates a new dot marker.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class Dot(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle appearance)
        {
            builder.Circle(new(), 2.0 * appearance.LineThickness, appearance.AsFilledMarker());
        }
    }
}
