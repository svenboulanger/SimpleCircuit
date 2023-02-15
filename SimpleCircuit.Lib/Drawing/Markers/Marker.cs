namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// An abstract class representing a marker.
    /// </summary>
    public abstract class Marker
    {
        /// <summary>
        /// Gets or sets the options of the marker.
        /// </summary>
        public GraphicOptions Options { get; set; }

        /// <summary>
        /// Gets the location of the marker.
        /// </summary>
        public Vector2 Location { get; set; }

        /// <summary>
        /// Gets the orientation of the marker.
        /// </summary>
        public Vector2 Orientation { get; set; }

        /// <summary>
        /// Creates a new <see cref="Marker"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="options">The options of the marker.</param>
        protected Marker(Vector2 location, Vector2 orientation, GraphicOptions options = null)
        {
            Location = location;
            Orientation = orientation;
            Options = options;
        }

        /// <summary>
        /// Draws the marker to the given drawing.
        /// </summary>
        /// <param name="drawing"></param>
        public void Draw(SvgDrawing drawing)
        {
            var orientation = Orientation;
            if (orientation.IsZero())
                orientation = new(1, 0);

            drawing.BeginTransform(new(Location, new(orientation.X, -orientation.Y, orientation.Y, orientation.X)));
            DrawMarker(drawing);
            drawing.EndTransform();
        }

        /// <summary>
        /// Draws the marker in local coordinates.
        /// The location is at (0, 0).
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        protected abstract void DrawMarker(SvgDrawing drawing);
    }
}
