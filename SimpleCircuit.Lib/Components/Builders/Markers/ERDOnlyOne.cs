namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "only one".
    /// </summary>
    /// <remarks>
    /// Creates an entity-relationship diagram marker for "only one".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class ERDOnlyOne(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            GraphicOptions options = new("marker", "erd", "onlyone");
            builder.Line(new(-2, -1.5), new(-2, 1.5), options);
            builder.Line(new(-4, -1.5), new(-4, 1.5), options);
        }
    }
}
