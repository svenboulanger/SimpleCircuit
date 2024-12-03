namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "one or many".
    /// </summary>
    /// <remarks>
    /// Creates an entity-relationship diagram marker for "one or many".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class ERDOneMany(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        private readonly static Vector2[] _points = [new(0, -1.5), new(-3, 0), new(0, 1.5)];

        /// <summary>
        /// Default graphic options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "one", "many");

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            builder.RequiredCSS.Add(".marker.erd.many { fill: transparent; }");
            builder.RequiredCSS.Add(".marker.erd.one { fill: white; }");
            builder.Polyline(_points, new("marker", "erd", "many"));
            builder.Line(new(-3, -1.5), new(-3, 1.5), new("marker", "erd", "one"));
        }
    }
}
