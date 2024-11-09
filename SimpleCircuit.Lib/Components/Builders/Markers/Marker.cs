namespace SimpleCircuit.Components.Builders.Markers
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
        public void Draw(IGraphicsBuilder builder)
        {
            builder.RequiredCSS.Add(".marker { fill: black; }");

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
