using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "one or many".
    /// </summary>
    /// <remarks>
    /// Creates an entity-relationship diagram marker for "one or many".
    /// </remarks>
    /// <param name="location"></param>
    /// <param name="orientation"></param>
    /// <param name="options"></param>
    public class ERDOneMany(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null) : Marker(location, orientation, options ?? DefaultOptions)
    {
        private readonly static Vector2[] _points = [new(0, -1.5), new(-3, 0), new(0, 1.5)];

        /// <summary>
        /// Default graphic options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "onemany");

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            builder.Polyline(_points, Options);
            builder.Line(new(-3, -1.5), new(-3, 1.5), Options);
        }
    }
}
