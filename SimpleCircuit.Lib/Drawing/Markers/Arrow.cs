﻿namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// An arrow marker.
    /// </summary>
    public class Arrow : Marker
    {
        private readonly static Vector2[] _points = new Vector2[] { new(-2.5, -1), new(0, 0), new(-2.5, 1) };

        /// <summary>
        /// Gets the default arrow options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "arrow");

        /// <summary>
        /// Creates a new arrow marker.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="options">The options.</param>
        public Arrow(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options)
        {
            Options ??= DefaultOptions;
        }

        /// <inheritdoc />
        protected override void DrawMarker(SvgDrawing drawing)
            => drawing.Polygon(_points, Options);
    }
}