namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "one".
    /// </summary>
    public class ERDOne : Marker
    {
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "one");

        /// <summary>
        /// Creates a new entity-relationship diagram marker for "one".
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="options">The options.</param>
        public ERDOne(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options ?? DefaultOptions)
        {
        }

        /// <inheritdoc />
        protected override void DrawMarker(SvgDrawing drawing)
        {
            drawing.Line(new(-2, -1.5), new(-2, 1.5), Options);
        }
    }
}
