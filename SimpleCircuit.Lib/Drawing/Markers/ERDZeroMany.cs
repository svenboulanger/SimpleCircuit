﻿namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "zero or many".
    /// </summary>
    public class ERDZeroMany : Marker
    {
        private readonly static Vector2[] _points = new Vector2[] { new(0, -1.5), new(-3, 0), new(0, 1.5) };

        /// <summary>
        /// Gets the default options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "zeromany");

        /// <summary>
        /// Creates a new entity-relationship diagram marker for "zero or many".
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="options">The options.</param>
        public ERDZeroMany(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options ?? DefaultOptions)
        {
        }

        /// <inheritdoc />
        protected override void DrawMarker(SvgDrawing drawing)
        {
            drawing.Polyline(_points, Options);
            drawing.Circle(new(-4.5, 0), 1.5, Options);
        }
    }
}