using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An arrow marker.
    /// </summary>
    /// <remarks>
    /// Creates a new arrow marker.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    /// <param name="options">The options.</param>
    public class Arrow(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null) : Marker(location, orientation, options ?? DefaultOptions)
    {
        private readonly static Vector2[] _points = [new(-2.5, -1), new(0, 0), new(-2.5, 1)];

        /// <summary>
        /// Gets the default arrow options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "arrow");

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder drawing)
            => drawing.Polygon(_points, Options);
    }
}
