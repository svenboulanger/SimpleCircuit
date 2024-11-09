namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// A dot marker.
    /// </summary>
    public class Dot : Marker
    {
        /// <summary>
        /// Gets the default options for a dot.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "dot");

        /// <summary>
        /// Creates a new dot marker.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="options">The options.</param>
        public Dot(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options)
        {
            Options ??= DefaultOptions;
        }

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
            => builder.Circle(new(), 1.0, Options);
    }
}
