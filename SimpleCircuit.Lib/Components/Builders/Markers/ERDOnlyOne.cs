using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "only one".
    /// </summary>
    /// <remarks>
    /// Creates an entity-relationship diagram marker for "only one".
    /// </remarks>
    /// <param name="location"></param>
    /// <param name="orientation"></param>
    /// <param name="options"></param>
    public class ERDOnlyOne(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null) : Marker(location, orientation, options ?? DefaultOptions)
    {
        /// <summary>
        /// Default graphic options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "onlyone");

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            builder.Line(new(-2, -1.5), new(-2, 1.5), Options);
            builder.Line(new(-4, -1.5), new(-4, 1.5), Options);
        }
    }
}
