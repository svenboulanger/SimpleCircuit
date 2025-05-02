namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An abstract class representing a marker.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="Marker"/>.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public abstract class Marker(Vector2 location, Vector2 orientation)
    {
        /// <summary>
        /// Gets the location of the marker.
        /// </summary>
        public Vector2 Location { get; set; } = location;

        /// <summary>
        /// Gets the orientation of the marker.
        /// </summary>
        public Vector2 Orientation { get; set; } = orientation;

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        public string Foreground { get; set; } = "black";

        /// <summary>
        /// Gets or sets the thickness of the wire for which the marker is drawn.
        /// </summary>
        public double Thickness { get; set; } = 0.5;

        /// <summary>
        /// Draws the marker to the given drawing.
        /// </summary>
        /// <param name="drawing"></param>
        public void Draw(IGraphicsBuilder builder)
        {
            var orientation = Orientation;
            if (orientation.IsZero())
                orientation = new(1, 0);

            builder.BeginTransform(new(Location, new(orientation.X, -orientation.Y, orientation.Y, orientation.X)));
            DrawMarker(builder);
            builder.EndTransform();
        }

        /// <summary>
        /// Draws the marker in local coordinates.
        /// The location is at (0, 0).
        /// </summary>
        /// <param name="builder">The drawing.</param>
        protected abstract void DrawMarker(IGraphicsBuilder builder);
    }
}
