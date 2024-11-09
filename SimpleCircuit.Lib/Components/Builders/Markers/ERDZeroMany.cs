using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "zero or many".
    /// </summary>
    /// <remarks>
    /// Creates a new entity-relationship diagram marker for "zero or many".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    /// <param name="options">The options.</param>
    public class ERDZeroMany(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null) : Marker(location, orientation, options ?? DefaultOptions)
    {
        private readonly static Vector2[] _points = [new(0, -1.5), new(-3, 0), new(0, 1.5)];

        /// <summary>
        /// Gets the default options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "zeromany");

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            builder.RequiredCSS.Add(".marker.erd { fill: white; }");
            builder.Polyline(_points, Options);
            builder.Circle(new(-4.5, 0), 1.5, Options);
        }
    }
}
