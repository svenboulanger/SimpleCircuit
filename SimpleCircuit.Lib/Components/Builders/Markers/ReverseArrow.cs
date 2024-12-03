namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// A reversed arrow marker.
    /// </summary>
    /// <remarks>
    /// Creates a new arrow marker.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class ReverseArrow(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        private readonly static Vector2[] _points = [new(0, -1), new(-2.5, 0), new(0, 1)];

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            builder.RequiredCSS.Add(".marker.arrow { fill: black; }");
            builder.Polygon(_points, new("marker", "arrow", "reverse"));
        }
    }
}
