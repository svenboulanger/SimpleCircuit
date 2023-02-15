namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// A slash marker.
    /// </summary>
    public class Slash : Marker
    {
        public static GraphicOptions DefaultOptions { get; } = new("marker", "slash");

        /// <summary>
        /// Creates a new slash marker.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="options">The options.</param>
        public Slash(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options)
        {
            Options = DefaultOptions;
        }

        /// <inheritdoc />
        protected override void DrawMarker(SvgDrawing drawing)
            => drawing.Line(new(-1, 2), new(1, -2), Options);
    }
}
