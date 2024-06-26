﻿namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// A minus marker.
    /// </summary>
    /// <remarks>
    /// Creates a new minus marker.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    /// <param name="options">The options.</param>
    public class Minus(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null) : Marker(location, orientation, options ?? DefaultOptions)
    {
        /// <summary>
        /// Gets the default marker options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "minus");

        /// <summary>
        /// Gets whether the plus should be drawn on the opposite side.
        /// </summary>
        public bool OppositeSide { get; set; }

        /// <inheritdoc />
        protected override void DrawMarker(SvgDrawing drawing)
        {
            Vector2 offset = OppositeSide ? new(-2.5, 3) : new(-2.5, -3);
            drawing.BeginTransform(new(offset, drawing.CurrentTransform.Matrix.Inverse));
            drawing.Line(new(-1, 0), new(1, 0), Options);
            drawing.EndTransform();
        }
    }
}
