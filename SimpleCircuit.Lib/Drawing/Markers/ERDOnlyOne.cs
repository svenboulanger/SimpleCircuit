namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "only one".
    /// </summary>
    public class ERDOnlyOne : Marker
    {
        /// <summary>
        /// Default graphic options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "onlyone");

        /// <summary>
        /// Creates an entity-relationship diagram marker for "only one".
        /// </summary>
        /// <param name="location"></param>
        /// <param name="orientation"></param>
        /// <param name="options"></param>
        public ERDOnlyOne(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options ?? DefaultOptions)
        {
        }

        /// <inheritdoc />
        protected override void DrawMarker(SvgDrawing drawing)
        {
            drawing.Line(new(-2, -1.5), new(-2, 1.5), Options);
            drawing.Line(new(-4, -1.5), new(-4, 1.5), Options);
        }
    }
}
