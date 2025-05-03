namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "one".
    /// </summary>
    /// <remarks>
    /// Creates a new entity-relationship diagram marker for "one".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    /// <param name="options">The options.</param>
    public class ERDOne(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, AppearanceOptions appearance)
        {
            var options = appearance.CreateMarkerOptions(hasFill: false);
            builder.Line(new Vector2(-4, -3) * appearance.LineThickness, new Vector2(-4, 3) * appearance.LineThickness, options);
        }
    }
}
