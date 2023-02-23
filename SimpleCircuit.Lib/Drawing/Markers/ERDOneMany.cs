namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "one or many".
    /// </summary>
    public class ERDOneMany : Marker
    {
        private readonly static Vector2[] _points = new Vector2[] { new(0, -1.5), new(-3, 0), new(0, 1.5) };

        /// <summary>
        /// Default graphic options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "onemany");

        /// <summary>
        /// Creates an entity-relationship diagram marker for "one or many".
        /// </summary>
        /// <param name="location"></param>
        /// <param name="orientation"></param>
        /// <param name="options"></param>
        public ERDOneMany(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options ?? DefaultOptions)
        {
        }

        /// <inheritdoc />
        protected override void DrawMarker(SvgDrawing drawing)
        {
            drawing.Polyline(_points, Options);
            drawing.Line(new(-3, -1.5), new(-3, 1.5), Options);
        }
    }
}
