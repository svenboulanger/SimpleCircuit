namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// A plus marker.
    /// </summary>
    public class Plus : Marker
    {
        /// <summary>
        /// Gets the default options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "plus");

        /// <summary>
        /// Gets whether the plus should be drawn on the opposite side.
        /// </summary>
        public bool OppositeSide { get; set; }

        /// <summary>
        /// Creates a new plus-sign marker.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="options">The graphic options.</param>
        public Plus(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options ?? DefaultOptions)
        {
        }

        /// <inheritdoc />
        protected override void DrawMarker(SvgDrawing drawing)
        {
            Vector2 offset = OppositeSide ? new(-2.5, 3) : new(-2.5, -3);
            drawing.BeginTransform(new(offset, drawing.CurrentTransform.Matrix.Inverse));
            drawing.Line(new(-1, 0), new(1, 0), Options);
            drawing.Line(new(0, -1), new(0, 1), Options);
            drawing.EndTransform();
        }
    }
}
