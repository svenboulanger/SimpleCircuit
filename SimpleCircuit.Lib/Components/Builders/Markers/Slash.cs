namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// A slash marker.
    /// </summary>
    /// <remarks>
    /// Creates a new slash marker.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class Slash(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
            => builder.Line(new(-1, 2), new(1, -2), new("marker", "slash"));
    }
}
