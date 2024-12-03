namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relation diagram marker for "zero or one".
    /// </summary>
    /// <remarks>
    /// Creates an entity-relationship diagram marker for "zero or one".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class ERDZeroOne(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            builder.RequiredCSS.Add(".marker.erd.zero { fill: white; }");
            builder.Circle(new(-5.5, 0), 1.5, new("marker", "erd", "zero"));
            builder.Line(new(-2, -1.5), new(-2, 1.5), new("marker", "erd", "one"));
        }
    }
}
