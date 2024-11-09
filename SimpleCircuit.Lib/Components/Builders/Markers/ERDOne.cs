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
    public class ERDOne(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null) : Marker(location, orientation, options ?? DefaultOptions)
    {
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "one");

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            builder.Line(new(-2, -1.5), new(-2, 1.5), Options);
        }
    }
}
