using SimpleCircuit.Components.Builders;

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
    /// <param name="options">The graphic options.</param>
    public class ERDZeroOne(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null) : Marker(location, orientation, options ?? DefaultOptions)
    {
        /// <summary>
        /// Default options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "zero-one");

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            builder.RequiredCSS.Add(".marker.erd { fill: white; }");
            builder.Circle(new(-5.5, 0), 1.5, Options);
            builder.Line(new(-2, -1.5), new(-2, 1.5), Options);
        }
    }
}
